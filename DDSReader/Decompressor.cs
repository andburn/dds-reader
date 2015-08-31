using System;
using System.Collections.Generic;
using System.Text;
using me.andburn.DDSReader.Utils;

namespace me.andburn.DDSReader
{
	public class Decompressor
	{
		public static byte[] Expand(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			System.Diagnostics.Debug.WriteLine(pixelFormat);
			
			byte[] rawData = null;

			switch(pixelFormat)
			{
			case PixelFormat.ARGB:
				rawData = DecompressRGBA(header, data, pixelFormat);
				break;

			case PixelFormat.RGB:
				rawData = DecompressRGB(header, data, pixelFormat);
				break;

			case PixelFormat.LUMINANCE:
			case PixelFormat.LUMINANCE_ALPHA:
				rawData = DecompressLum(header, data, pixelFormat);
				break;

			case PixelFormat.DXT1:
				rawData = DecompressDXT1(header, data, pixelFormat);
				break;

			case PixelFormat.DXT2:
				rawData = DecompressDXT2(header, data, pixelFormat);
				break;

			case PixelFormat.DXT3:
				rawData = DecompressDXT3(header, data, pixelFormat);
				break;

			case PixelFormat.DXT4:
				rawData = DecompressDXT3(header, data, pixelFormat);
				break;

			case PixelFormat.DXT5:
				rawData = DecompressDXT5(header, data, pixelFormat);
				break;

			case PixelFormat.THREEDC:
				rawData = Decompress3Dc(header, data, pixelFormat);
				break;

			case PixelFormat.ATI1N:
				rawData = DecompressAti1n(header, data, pixelFormat);
				break;

			case PixelFormat.RXGB:
				rawData = DecompressRXGB(header, data, pixelFormat);
				break;

			case PixelFormat.R16F:
			case PixelFormat.G16R16F:
			case PixelFormat.A16B16G16R16F:
			case PixelFormat.R32F:
			case PixelFormat.G32R32F:
			case PixelFormat.A32B32G32R32F:
				rawData = DecompressFloat(header, data, pixelFormat);
				break;

			default:
				throw new UnknownFileFormatException();
			}

			return rawData;
		}

		private static byte[] DecompressDXT2(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// Can do color & alpha same as dxt3, but color is pre-multiplied
			// so the result will be wrong unless corrected.
			byte[] rawData = DecompressDXT3(header, data, pixelFormat);
			Helper.CorrectPremult((uint)(width * height * depth), ref rawData);

			return rawData;
		}

		private static byte[] DecompressDXT4(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// Can do color & alpha same as dxt5, but color is pre-multiplied
			// so the result will be wrong unless corrected.
			byte[] rawData = DecompressDXT5(header, data, pixelFormat);
			Helper.CorrectPremult((uint)(width * height * depth), ref rawData);

			return rawData;
		}

		private static unsafe byte[] DecompressDXT1(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// DXT1 decompressor
			Colour8888[] colours = new Colour8888[4];
			Colour8888 col;
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			ushort colour0, colour1;
			uint bitmask, offset;
			int i, j, k, x, y, z, select;

			colours[0].alpha = 0xFF;
			colours[1].alpha = 0xFF;
			colours[2].alpha = 0xFF;

			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							colour0 = *((ushort*)temp);
							colour1 = *((ushort*)(temp + 2));
							Helper.DxtcReadColor(colour0, ref colours[0]);
							Helper.DxtcReadColor(colour1, ref colours[1]);

							bitmask = ((uint*)temp)[1];
							temp += 8;

							if(colour0 > colour1)
							{
								// Four-color block: derive the other two colors.
								// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
								// These 2-bit codes correspond to the 2-bit fields
								// stored in the 64-bit block.
								colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
								colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
								colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
								//colours[2].alpha = 0xFF;

								colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
								colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
								colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
								colours[3].alpha = 0xFF;
							}
							else
							{
								// Three-color block: derive the other color.
								// 00 = color_0,  01 = color_1,  10 = color_2,
								// 11 = transparent.
								// These 2-bit codes correspond to the 2-bit fields 
								// stored in the 64-bit block. 
								colours[2].blue = (byte)((colours[0].blue + colours[1].blue) / 2);
								colours[2].green = (byte)((colours[0].green + colours[1].green) / 2);
								colours[2].red = (byte)((colours[0].red + colours[1].red) / 2);
								//colours[2].alpha = 0xFF;

								colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
								colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
								colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
								colours[3].alpha = 0x00;
							}

							for(j = 0, k = 0; j < 4; j++)
							{
								for(i = 0; i < 4; i++, k++)
								{
									select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									col = colours[select];
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = (byte)col.red;
										rawData[offset + 1] = (byte)col.green;
										rawData[offset + 2] = (byte)col.blue;
										rawData[offset + 3] = (byte)col.alpha;
									}
								}
							}
						}
					}
				}
			}

			return rawData;
		}
		
		private static unsafe byte[] DecompressDXT3(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			// DXT3 decompressor
			Colour8888[] colours = new Colour8888[4];
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint bitmask, offset;
			int i, j, k, x, y, z, select;
			ushort word;
			byte* alpha; //temp;

			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;

				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							alpha = temp;
							temp += 8;

							Helper.DxtcReadColors(temp, ref colours);
							temp += 4;

							bitmask = ((uint*)temp)[1];
							temp += 4;

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							colours[3].alpha = 0xFF;

							for(j = 0, k = 0; j < 4; j++)
							{
								for(i = 0; i < 4; k++, i++)
								{
									select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = (byte)colours[select].red;
										rawData[offset + 1] = (byte)colours[select].green;
										rawData[offset + 2] = (byte)colours[select].blue;
									}
								}
							}

							for(j = 0; j < 4; j++)
							{
								word = (ushort)(alpha[2 * j] | (alpha[2 * j + 1] << 8)); //(alpha[2 * j] + 256 * alpha[2 * j + 1]);
								for(i = 0; i < 4; i++)
								{
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)(word & 0x0F);
										rawData[offset] = (byte)(rawData[offset] | (rawData[offset] << 4));
									}
									word >>= 4;
								}
							}
						}
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressDXT5(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			Colour8888[] colours = new Colour8888[4];
			Colour8888 col;
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint bitmask, offset;
			int i, j, k, x, y, z, select;
			ushort bits;
			ushort[] alphas = new ushort[8];
			byte* alphamask;

			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							if(y >= height || x >= width)
								break;

							alphas[0] = temp[0];
							alphas[1] = temp[1];
							alphamask = (temp + 2);
							temp += 8;

							Helper.DxtcReadColors(temp, ref colours);
							bitmask = ((uint*)temp)[1];
							temp += 8;

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							//colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							//colours[3].alpha = 0xFF;

							k = 0;
							for(j = 0; j < 4; j++)
							{
								for(i = 0; i < 4; k++, i++)
								{
									select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									col = colours[select];
									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset] = (byte)col.red;
										rawData[offset + 1] = (byte)col.green;
										rawData[offset + 2] = (byte)col.blue;
									}
								}
							}
							
							// 8-alpha or 6-alpha block?
							if(alphas[0] > alphas[1])
							{
								// 8-alpha block:  derive the other six alphas.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (ushort)((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
								alphas[3] = (ushort)((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
								alphas[4] = (ushort)((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
								alphas[5] = (ushort)((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
								alphas[6] = (ushort)((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
								alphas[7] = (ushort)((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
							}
							else
							{
								// 6-alpha block.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (ushort)((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
								alphas[3] = (ushort)((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
								alphas[4] = (ushort)((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
								alphas[5] = (ushort)((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
								alphas[6] = 0x00; // Bit code 110
								alphas[7] = 0xFF; // Bit code 111
							}
							
							// Note: Have to separate the next two loops,
							// it operates on a 6-byte system.
							
							// First three bytes
							bits = (ushort)((alphamask[0]) | (alphamask[1] << 8) | (alphamask[2] << 16));
							for(j = 0; j < 2; j++)
							{
								for(i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}

							// Last three bytes
							bits = (ushort)((alphamask[3]) | (alphamask[4] << 8) | (alphamask[5] << 16));
							for(j = 2; j < 4; j++)
							{
								for(i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
										rawData[offset] = (byte)alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}							
						}
					}
				}
			}

			return rawData;
		}

		private static unsafe byte[] DecompressRGB(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint valMask = (uint)((1 << (int)header.pixelformat.rgbbitcount) - 1);
			uint pixSize = (uint)(((int)header.pixelformat.rgbbitcount + 7) / 8);
			int rShift1 = 0; int rMul = 0; int rShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
			int gShift1 = 0; int gMul = 0; int gShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
			int bShift1 = 0; int bMul = 0; int bShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				while(pixnum-- > 0)
				{
					uint px = *((uint*)temp) & valMask;
					temp += pixSize;
					uint pxc = px & header.pixelformat.rbitmask;
					rawData[offset] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
					pxc = px & header.pixelformat.gbitmask;
					rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
					pxc = px & header.pixelformat.bbitmask;
					rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
					rawData[offset + 3] = 0xff;
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressRGBA(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint valMask = (uint)((header.pixelformat.rgbbitcount == 32) ? ~0 : (1 << (int)header.pixelformat.rgbbitcount) - 1);
			// Funny x86s, make 1 << 32 == 1
			uint pixSize = (header.pixelformat.rgbbitcount + 7) / 8;
			int rShift1 = 0; int rMul = 0; int rShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
			int gShift1 = 0; int gMul = 0; int gShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
			int bShift1 = 0; int bMul = 0; int bShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);
			int aShift1 = 0; int aMul = 0; int aShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.alphabitmask, ref aShift1, ref aMul, ref aShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;

				while(pixnum-- > 0)
				{
					uint px = *((uint*)temp) & valMask;
					temp += pixSize;
					uint pxc = px & header.pixelformat.rbitmask;
					rawData[offset] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
					pxc = px & header.pixelformat.gbitmask;
					rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
					pxc = px & header.pixelformat.bbitmask;
					rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
					pxc = px & header.pixelformat.alphabitmask;
					rawData[offset + 3] = (byte)(((pxc >> aShift1) * aMul) >> aShift2);
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] Decompress3Dc(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint bitmask, bitmask2;
			int offset, currentOffset;
			int x, y, z, i, j, k, t1, t2;
			byte* temp2;
			byte[] yColours = new byte[8];
			byte[] xColours = new byte[8];

			offset = 0;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							temp2 = temp + 8;

							//Read Y palette
							t1 = yColours[0] = temp[0];
							t2 = yColours[1] = temp[1];
							temp += 2;
							if(t1 > t2)
								for(i = 2; i < 8; ++i)
									yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for(i = 2; i < 6; ++i)
									yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								yColours[6] = 0;
								yColours[7] = 255;
							}

							// Read X palette
							t1 = xColours[0] = temp2[0];
							t2 = xColours[1] = temp2[1];
							temp2 += 2;
							if(t1 > t2)
								for(i = 2; i < 8; ++i)
									xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for(i = 2; i < 6; ++i)
									xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								xColours[6] = 0;
								xColours[7] = 255;
							}

							//decompress pixel data
							currentOffset = offset;
							for(k = 0; k < 4; k += 2)
							{
								// First three bytes
								bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
								bitmask2 = ((uint)(temp2[0]) << 0) | ((uint)(temp2[1]) << 8) | ((uint)(temp2[2]) << 16);
								for(j = 0; j < 2; j++)
								{
									// only put pixels out < height
									if((y + k + j) < height)
									{
										for(i = 0; i < 4; i++)
										{
											// only put pixels out < width
											if(((x + i) < width))
											{
												int t;
												byte tx, ty;

												t1 = currentOffset + (x + i) * 3;
												rawData[t1 + 1] = ty = yColours[bitmask & 0x07];
												rawData[t1 + 0] = tx = xColours[bitmask2 & 0x07];

												//calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
												t = 127 * 128 - (tx - 127) * (tx - 128) - (ty - 127) * (ty - 128);
												if(t > 0)
													rawData[t1 + 2] = (byte)(Math.Sqrt(t) + 128);
												else
													rawData[t1 + 2] = 0x7F;
											}
											bitmask >>= 3;
											bitmask2 >>= 3;
										}
										currentOffset += bps;
									}
								}
								temp += 3;
								temp2 += 3;
							}

							//skip bytes that were read via Temp2
							temp += 8;
						}
						offset += bps * 4;
					}
				}
			}

			return rawData;
		}

		private static unsafe byte[] DecompressAti1n(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint bitmask, offset, currOffset;
			int i, j, k, x, y, z, t1, t2;
			byte[] colours = new byte[8];

			offset = 0;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							//Read palette
							t1 = colours[0] = temp[0];
							t2 = colours[1] = temp[1];
							temp += 2;
							if(t1 > t2)
								for(i = 2; i < 8; ++i)
									colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
							else
							{
								for(i = 2; i < 6; ++i)
									colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
								colours[6] = 0;
								colours[7] = 255;
							}

							//decompress pixel data
							currOffset = offset;
							for(k = 0; k < 4; k += 2)
							{
								// First three bytes
								bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
								for(j = 0; j < 2; j++)
								{
									// only put pixels out < height
									if((y + k + j) < height)
									{
										for(i = 0; i < 4; i++)
										{
											// only put pixels out < width
											if(((x + i) < width))
											{
												t1 = (int)(currOffset + (x + i));
												rawData[t1] = colours[bitmask & 0x07];
											}
											bitmask >>= 3;
										}
										currOffset += (uint)bps;
									}
								}
								temp += 3;
							}
						}
						offset += (uint)(bps * 4);
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressLum(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			int lShift1 = 0; int lMul = 0; int lShift2 = 0;
			Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref lShift1, ref lMul, ref lShift2);

			int offset = 0;
			int pixnum = width * height * depth;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				while(pixnum-- > 0)
				{
					byte px = *(temp++);
					rawData[offset] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 1] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 2] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					rawData[offset + 3] = (byte)(((px >> lShift1) * lMul) >> lShift2);
					offset += 4;
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressRXGB(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			int x, y, z, i, j, k, select;
			Colour565 color_0 = new Colour565();
			Colour565 color_1 = new Colour565();
			Colour8888 col;
			Colour8888[] colours = new Colour8888[4];
			uint bitmask, offset;
			byte[] alphas = new byte[8];
			byte* alphamask;
			uint bits;

			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;

				for(z = 0; z < depth; z++)
				{
					for(y = 0; y < height; y += 4)
					{
						for(x = 0; x < width; x += 4)
						{
							if(y >= height || x >= width)
								break;
							alphas[0] = temp[0];
							alphas[1] = temp[1];
							alphamask = temp + 2;
							temp += 8;

							Helper.DxtcReadColors(temp, ref color_0, ref color_1);
							temp += 4;

							bitmask = ((uint*)temp)[1];
							temp += 4;

							colours[0].red = (byte)(color_0.red << 3);
							colours[0].green = (byte)(color_0.green << 2);
							colours[0].blue = (byte)(color_0.blue << 3);
							colours[0].alpha = 0xFF;

							colours[1].red = (byte)(color_1.red << 3);
							colours[1].green = (byte)(color_1.green << 2);
							colours[1].blue = (byte)(color_1.blue << 3);
							colours[1].alpha = 0xFF;

							// Four-color block: derive the other two colors.    
							// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
							// These 2-bit codes correspond to the 2-bit fields 
							// stored in the 64-bit block.
							colours[2].blue = (byte)((2 * colours[0].blue + colours[1].blue + 1) / 3);
							colours[2].green = (byte)((2 * colours[0].green + colours[1].green + 1) / 3);
							colours[2].red = (byte)((2 * colours[0].red + colours[1].red + 1) / 3);
							colours[2].alpha = 0xFF;

							colours[3].blue = (byte)((colours[0].blue + 2 * colours[1].blue + 1) / 3);
							colours[3].green = (byte)((colours[0].green + 2 * colours[1].green + 1) / 3);
							colours[3].red = (byte)((colours[0].red + 2 * colours[1].red + 1) / 3);
							colours[3].alpha = 0xFF;

							k = 0;
							for(j = 0; j < 4; j++)
							{
								for(i = 0; i < 4; i++, k++)
								{
									select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
									col = colours[select];

									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
										rawData[offset + 0] = col.red;
										rawData[offset + 1] = col.green;
										rawData[offset + 2] = col.blue;
									}
								}
							}

							// 8-alpha or 6-alpha block?    
							if(alphas[0] > alphas[1])
							{
								// 8-alpha block:  derive the other six alphas.    
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);	// bit code 010
								alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);	// bit code 011
								alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);	// bit code 100
								alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);	// bit code 101
								alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);	// bit code 110
								alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);	// bit code 111
							}
							else
							{
								// 6-alpha block.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);	// Bit code 010
								alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);	// Bit code 011
								alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);	// Bit code 100
								alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);	// Bit code 101
								alphas[6] = 0x00;										// Bit code 110
								alphas[7] = 0xFF;										// Bit code 111
							}

							// Note: Have to separate the next two loops,
							//	it operates on a 6-byte system.
							// First three bytes
							bits = *((uint*)alphamask);
							for(j = 0; j < 2; j++)
							{
								for(i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 0);
										rawData[offset] = alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}

							// Last three bytes
							bits = *((uint*)&alphamask[3]);
							for(j = 2; j < 4; j++)
							{
								for(i = 0; i < 4; i++)
								{
									// only put pixels out < width or height
									if(((x + i) < width) && ((y + j) < height))
									{
										offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 0);
										rawData[offset] = alphas[bits & 0x07];
									}
									bits >>= 3;
								}
							}
						}
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressFloat(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
			int i, j, size;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				fixed(byte* destPtr = rawData)
				{
					byte* destData = destPtr;
					switch(pixelFormat)
					{
					case PixelFormat.R32F:  // Red float, green = blue = max
						size = width * height * depth * 3;
						for(i = 0, j = 0; i < size; i += 3, j++)
						{
							((float*)destData)[i] = ((float*)temp)[j];
							((float*)destData)[i + 1] = 1.0f;
							((float*)destData)[i + 2] = 1.0f;
						}
						break;

					case PixelFormat.A32B32G32R32F:  // Direct copy of float RGBA data
						Array.Copy(data, rawData, data.Length);
						break;

					case PixelFormat.G32R32F:  // Red float, green float, blue = max
						size = width * height * depth * 3;
						for(i = 0, j = 0; i < size; i += 3, j += 2)
						{
							((float*)destData)[i] = ((float*)temp)[j];
							((float*)destData)[i + 1] = ((float*)temp)[j + 1];
							((float*)destData)[i + 2] = 1.0f;
						}
						break;

					case PixelFormat.R16F:  // Red float, green = blue = max
						size = width * height * depth * bpp;
						Helper.ConvR16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
						break;

					case PixelFormat.A16B16G16R16F:  // Just convert from half to float.
						size = width * height * depth * bpp;
						Helper.ConvFloat16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
						break;

					case PixelFormat.G16R16F:  // Convert from half to float, set blue = max.
						size = width * height * depth * bpp;
						Helper.ConvG16R16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
						break;

					default:
						break;
					}
				}
			}

			return rawData;
		}

		private static unsafe byte[] DecompressARGB(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			if(Helper.Check16BitComponents(header))
				return DecompressARGB16(header, data, pixelFormat);

			int sizeOfData = (int)((header.width * header.pixelformat.rgbbitcount / 8) * header.height * header.depth);
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			if((pixelFormat == PixelFormat.LUMINANCE) && (header.pixelformat.rgbbitcount == 16) && (header.pixelformat.rbitmask == 0xFFFF))
			{
				Array.Copy(data, rawData, data.Length);
				return rawData;
			}

			uint readI = 0, tempBpp;
			int i;
			uint redL = 0, redR = 0;
			uint greenL = 0, greenR = 0;
			uint blueL = 0, blueR = 0;
			uint alphaL = 0, alphaR = 0;

			Helper.GetBitsFromMask(header.pixelformat.rbitmask, ref redL, ref redR);
			Helper.GetBitsFromMask(header.pixelformat.gbitmask, ref greenL, ref greenR);
			Helper.GetBitsFromMask(header.pixelformat.bbitmask, ref blueL, ref blueR);
			Helper.GetBitsFromMask(header.pixelformat.alphabitmask, ref alphaL, ref alphaR);
			tempBpp = header.pixelformat.rgbbitcount / 8;

			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;

				for(i = 0; i < sizeOfData; i += bpp)
				{
					//@TODO: This is SLOOOW...
					//but the old version crashed in release build under
					//winxp (and xp is right to stop this code - I always
					//wondered that it worked the old way at all)
					if(sizeOfData - i < 4)
					{
						//less than 4 byte to write?
						if(tempBpp == 3)
						{
							//this branch is extra-SLOOOW
							readI = (uint)(*temp | ((*(temp + 1)) << 8) | ((*(temp + 2)) << 16));
						}
						else if(tempBpp == 1)
							readI = *((byte*)temp);
						else if(tempBpp == 2)
							readI = (uint)(temp[0] | (temp[1] << 8));
					}
					else
						readI = (uint)(temp[0] | (temp[1] << 8) | (temp[2] << 16) | (temp[3] << 24));
					temp += tempBpp;

					rawData[i] = (byte)((((int)readI & (int)header.pixelformat.rbitmask) >> (int)redR) << (int)redL);

					if(bpp >= 3)
					{
						rawData[i + 1] = (byte)((((int)readI & (int)header.pixelformat.gbitmask) >> (int)greenR) << (int)greenL);
						rawData[i + 2] = (byte)((((int)readI & header.pixelformat.bbitmask) >> (int)blueR) << (int)blueL);

						if(bpp == 4)
						{
							rawData[i + 3] = (byte)((((int)readI & (int)header.pixelformat.alphabitmask) >> (int)alphaR) << (int)alphaL);
							if(alphaL >= 7)
							{
								rawData[i + 3] = (byte)(rawData[i + 3] != 0 ? 0xFF : 0x00);
							}
							else if(alphaL >= 4)
							{
								rawData[i + 3] = (byte)(rawData[i + 3] | (rawData[i + 3] >> 4));
							}
						}
					}
					else if(bpp == 2)
					{
						rawData[i + 1] = (byte)((((int)readI & (int)header.pixelformat.alphabitmask) >> (int)alphaR) << (int)alphaL);
						if(alphaL >= 7)
						{
							rawData[i + 1] = (byte)(rawData[i + 1] != 0 ? 0xFF : 0x00);
						}
						else if(alphaL >= 4)
						{
							rawData[i + 1] = (byte)(rawData[i + 1] | (rawData[i + 3] >> 4));
						}
					}
				}
			}
			return rawData;
		}

		private static unsafe byte[] DecompressARGB16(DDSStruct header, byte[] data, PixelFormat pixelFormat)
		{
			// allocate bitmap
			int bpp = (int)(Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount));
			int bps = (int)(header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
			int sizeofplane = (int)(bps * header.height);
			int width = (int)header.width;
			int height = (int)header.height;
			int depth = (int)header.depth;

			int sizeOfData = (int)((header.width * header.pixelformat.rgbbitcount / 8) * header.height * header.depth);
			byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

			uint readI = 0, tempBpp = 0;
			int i;
			uint redL = 0, redR = 0;
			uint greenL = 0, greenR = 0;
			uint blueL = 0, blueR = 0;
			uint alphaL = 0, alphaR = 0;
			uint redPad = 0, greenPad = 0, bluePad = 0, alphaPad = 0;

			Helper.GetBitsFromMask(header.pixelformat.rbitmask, ref redL, ref redR);
			Helper.GetBitsFromMask(header.pixelformat.gbitmask, ref greenL, ref greenR);
			Helper.GetBitsFromMask(header.pixelformat.bbitmask, ref blueL, ref blueR);
			Helper.GetBitsFromMask(header.pixelformat.alphabitmask, ref alphaL, ref alphaR);
			redPad = 16 - Helper.CountBitsFromMask(header.pixelformat.rbitmask);
			greenPad = 16 - Helper.CountBitsFromMask(header.pixelformat.gbitmask);
			bluePad = 16 - Helper.CountBitsFromMask(header.pixelformat.bbitmask);
			alphaPad = 16 - Helper.CountBitsFromMask(header.pixelformat.alphabitmask);

			redL = redL + redPad;
			greenL = greenL + greenPad;
			blueL = blueL + bluePad;
			alphaL = alphaL + alphaPad;

			tempBpp = header.pixelformat.rgbbitcount / 8;
			fixed(byte* bytePtr = data)
			{
				byte* temp = bytePtr;
				fixed(byte* destPtr = rawData)
				{
					byte* destData = destPtr;
					for(i = 0; i < sizeOfData / 2; i += bpp)
					{
						//@TODO: This is SLOOOW...
						//but the old version crashed in release build under
						//winxp (and xp is right to stop this code - I always
						//wondered that it worked the old way at all)
						if(sizeOfData - i < 4)
						{
							//less than 4 byte to write?
							if(tempBpp == 3)
							{
								//this branch is extra-SLOOOW
								readI = (uint)(*temp | ((*(temp + 1)) << 8) | ((*(temp + 2)) << 16));
							}
							else if(tempBpp == 1)
								readI = *((byte*)temp);
							else if(tempBpp == 2)
								readI = (uint)(temp[0] | (temp[1] << 8));
						}
						else
							readI = (uint)(temp[0] | (temp[1] << 8) | (temp[2] << 16) | (temp[3] << 24));
						temp += tempBpp;

						((ushort*)destData)[i + 2] = (ushort)((((int)readI & (int)header.pixelformat.rbitmask) >> (int)redR) << (int)redL);

						if(bpp >= 3)
						{
							((ushort*)destData)[i + 1] = (ushort)((((int)readI & (int)header.pixelformat.gbitmask) >> (int)greenR) << (int)greenL);
							((ushort*)destData)[i] = (ushort)((((int)readI & (int)header.pixelformat.bbitmask) >> (int)blueR) << (int)blueL);

							if(bpp == 4)
							{
								((ushort*)destData)[i + 3] = (ushort)((((int)readI & (int)header.pixelformat.alphabitmask) >> (int)alphaR) << (int)alphaL);
								if(alphaL >= 7)
								{
									((ushort*)destData)[i + 3] = (ushort)(((ushort*)destData)[i + 3] != 0 ? 0xFF : 0x00);
								}
								else if(alphaL >= 4)
								{
									((ushort*)destData)[i + 3] = (ushort)(((ushort*)destData)[i + 3] | (((ushort*)destData)[i + 3] >> 4));
								}
							}
						}
						else if(bpp == 2)
						{
							((ushort*)destData)[i + 1] = (ushort)((((int)readI & (int)header.pixelformat.alphabitmask) >> (int)alphaR) << (int)alphaL);
							if(alphaL >= 7)
							{
								((ushort*)destData)[i + 1] = (ushort)(((ushort*)destData)[i + 1] != 0 ? 0xFF : 0x00);
							}
							else if(alphaL >= 4)
							{
								((ushort*)destData)[i + 1] = (ushort)(((ushort*)destData)[i + 1] | (rawData[i + 3] >> 4));
							}
						}
					}
				}
			}
			return rawData;
		}
	}
}
