using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using me.andburn.DDSReader.Utils;

namespace me.andburn.DDSReader
{
	public class DDSImage
	{
        /// <summary>
        /// A space-seperated list of supported image encoders.
        /// </summary>

		private Bitmap _bitmap;

		public Bitmap BitmapImage
		{
			get { return this._bitmap; }
		}

		public DDSImage(byte[] ddsImage)
		{
			if(ddsImage == null) return;
			if(ddsImage.Length == 0) return;

			using(MemoryStream stream = new MemoryStream(ddsImage.Length))
			{
				stream.Write(ddsImage, 0, ddsImage.Length);
				stream.Seek(0, SeekOrigin.Begin);

				using(BinaryReader reader = new BinaryReader(stream))
				{
					this.Parse(reader);
				}
			}
		}

		public DDSImage(Stream ddsImage)
		{
			if(ddsImage == null) return;
			if(!ddsImage.CanRead) return;

			using(BinaryReader reader = new BinaryReader(ddsImage))
			{
				this.Parse(reader);
			}
		}

		/*public DDSImage(byte[] ddsimage)
		{
			// creates a new DDSImage from a byte[] containing a DDS Image.
			MemoryStream ms = new MemoryStream(ddsimage.Length);
			ms.Write(ddsimage, 0, ddsimage.Length);
			ms.Seek(0, SeekOrigin.Begin);

			br = new BinaryReader(ms);
			this.signature = br.ReadBytes(4);
			
			if (!IsByteArrayEqual(this.signature, DDS_HEADER)) {
				System.Console.WriteLine("Got header of '" + ASCIIEncoding.ASCII.GetString(this.signature, 0, this.signature.Length) + "'.");
				
				throw new NotADDSImageException();
			}
			
			//System.Console.WriteLine("Got dds header okay");
			
			// now read in the rest
			this.size1 = br.ReadUInt32();
			this.flags1 = br.ReadUInt32();
			this.height = br.ReadUInt32();
			this.width = br.ReadUInt32();
			this.linearsize = br.ReadUInt32();
			this.depth = br.ReadUInt32();
			this.mipmapcount = br.ReadUInt32();
			this.alphabitdepth = br.ReadUInt32();
			
			// skip next 10 uints
			for (int x=0; x<10; x++) {
				br.ReadUInt32();
			}
			
			this.size2 = br.ReadUInt32();
			this.flags2 = br.ReadUInt32();
			this.fourcc = br.ReadUInt32();
			this.rgbbitcount = br.ReadUInt32();
			this.rbitmask = br.ReadUInt32();
			this.gbitmask = br.ReadUInt32();
            this.bbitmask = br.ReadUInt32();
			this.alphabitmask = br.ReadUInt32();
			this.ddscaps1 = br.ReadUInt32();
			this.ddscaps2 = br.ReadUInt32();
			this.ddscaps3 = br.ReadUInt32();
			this.ddscaps4 = br.ReadUInt32();
			this.texturestage = br.ReadUInt32();
            
			
			// patches for stuff
			if (this.depth == 0) {
				this.depth = 1;
			}

			if ((this.flags2 & DDS_FOURCC) > 0) {
				blocksize = ((this.width+3)/4) * ((this.height+3)/4) * this.depth;
				
				switch (this.fourcc) {
					case FOURCC_DXT1:
						CompFormat = PixelFormat.DXT1;
						blocksize *= 8;
						break;
					
					case FOURCC_DXT2:
						CompFormat = PixelFormat.DXT2;
						blocksize *= 16;
						break;
					
					case FOURCC_DXT3:
						CompFormat = PixelFormat.DXT3;
						blocksize *= 16;
						break;
						
					case FOURCC_DXT4:
						CompFormat = PixelFormat.DXT4;
						blocksize *= 16;
						break;
					
					case FOURCC_DXT5:
						CompFormat = PixelFormat.DXT5;
						blocksize *= 16;
						break;
					
					case FOURCC_ATI1:
						CompFormat = PixelFormat.ATI1N;
						blocksize *= 8;
						break;
					
					case FOURCC_ATI2:
						CompFormat = PixelFormat.THREEDC;
						blocksize *= 16;
						break;
					
					case FOURCC_RXGB:
						CompFormat = PixelFormat.RXGB;
						blocksize *= 16;
						break;
					
					case FOURCC_DOLLARNULL:
						CompFormat = PixelFormat.A16B16G16R16;
						blocksize = this.width * this.height * this.depth * 8;
						break;
					
					case FOURCC_oNULL:
						CompFormat = PixelFormat.R16F;
						blocksize = this.width * this.height * this.depth * 2;
						break;
					
					case FOURCC_pNULL:
						CompFormat = PixelFormat.G16R16F;
						blocksize = this.width * this.height * this.depth * 4;
						break;
						
					case FOURCC_qNULL:
						CompFormat = PixelFormat.A16B16G16R16F;
						blocksize = this.width * this.height * this.depth * 8;
						break;
					
					case FOURCC_rNULL:
						CompFormat = PixelFormat.R32F;
						blocksize = this.width * this.height * this.depth * 4;
						break;
					
					case FOURCC_sNULL:
						CompFormat = PixelFormat.G32R32F;
						blocksize = this.width * this.height * this.depth * 8;
						break;
					
					case FOURCC_tNULL:
						CompFormat = PixelFormat.A32B32G32R32F;
						blocksize = this.width * this.height * this.depth * 16;
						break;
						
					default:
						CompFormat = PixelFormat.UNKNOWN;
						blocksize *= 16;
						break;
				} // switch
			} else {
				// uncompressed image
				if ((this.flags2 & DDS_LUMINANCE) > 0) {
					if ((this.flags2 & DDS_ALPHAPIXELS) > 0) {
						CompFormat = PixelFormat.LUMINANCE_ALPHA;
					} else {
						CompFormat = PixelFormat.LUMINANCE;
					}
				} else {
					if ((this.flags2 & DDS_ALPHAPIXELS) > 0) {
						CompFormat = PixelFormat.ARGB;
					} else {
						CompFormat = PixelFormat.RGB;
					}
				}
				
				blocksize = (this.width * this.height * this.depth * (this.rgbbitcount >> 3));
			}			
			
			if (CompFormat == PixelFormat.UNKNOWN) {
				throw new InvalidFileHeaderException();
			}
			
			if ((this.flags1 & (DDS_LINEARSIZE | DDS_PITCH))==0
				|| this.linearsize == 0) {
				this.flags1 |= DDS_LINEARSIZE;
				this.linearsize = blocksize;
			}
			
			
			//System.Console.WriteLine(String.Format("Image Size: {0}x{1}, Pixel Format: {2}, Blocksize: {3}", this.width, this.height, this.CompFormat, this.blocksize));
			
			// get image data
			this.ReadData();
			
			//System.Console.WriteLine(String.Format("Compressed data size: {0}/{1} bytes", this.compsize, this.compdata.Length));
			
			// allocate bitmap
			this.bpp = this.PixelFormatToBpp(this.CompFormat);
			this.bps = this.width * this.bpp * this.PixelFormatToBpc(this.CompFormat);
			this.sizeofplane = this.bps * this.height;
			this.rawidata = new byte[this.depth * this.sizeofplane + this.height * this.bps + this.width * this.bpp];
			
			// decompress
			switch (this.CompFormat) {
				case PixelFormat.ARGB:
				case PixelFormat.RGB:
				case PixelFormat.LUMINANCE:
				case PixelFormat.LUMINANCE_ALPHA:
					this.DecompressARGB();
					break;
				
				case PixelFormat.DXT1:
					this.DecompressDXT1();
					break;
					
				case PixelFormat.DXT3:
					this.DecompressDXT3();
					break;

				case PixelFormat.DXT5:
					this.DecompressDXT5();
					break;

				default:
					throw new UnknownFileFormatException();
			}
			
			this.img = new Bitmap((int)this.width, (int)this.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

#if SAFE_MODE
            // now fill bitmap with raw image datas.  this is really slow.
			// but only on windows/microsoft's .net clr.  it's fast in mono.
            // should find a better way to do this.

            for (int y=0; y<this.height; y++) {
                for (int x=0; x<this.width; x++) {
                    // draw
                    ulong pos = (ulong)(((y*this.width)+x)*4);
                    this.img.SetPixel(x, y, Color.FromArgb(this.rawidata[pos+3], this.rawidata[pos], this.rawidata[pos+1], this.rawidata[pos+2]));
                }
            }
#else
            // new optimised Bitmap creation routine.  Based on Bitmap->Gdk.Pixbuf conversion code
            // thanks to bratsche (Cody Russell) on #mono for the help!

            BitmapData data = this.img.LockBits(new Rectangle(0, 0,
                this.img.Width,
                this.img.Height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr scan = data.Scan0;
            int size = this.img.Width * this.img.Height * 4;
            //byte[] bdata = new byte[size];
            
            unsafe
            {
                byte* p = (byte*)scan;
                for (int i = 0; i < size; i += 4)
                {
                    // iterate through bytes.
                    // Bitmap stores it's data in RGBA order.
                    // DDS stores it's data in BGRA order.
                    p[i] = this.rawidata[i + 2]; // blue
                    p[i + 1] = this.rawidata[i + 1]; // green
                    p[i + 2] = this.rawidata[i];   // red
                    p[i + 3] = this.rawidata[i + 3]; // alpha
                }
            }

            this.img.UnlockBits(data);
#endif


            // cleanup
			this.rawidata = null;
			this.compdata = null;
			//this.img.Save("/home/michael/idata.bmp");
			
			
		}
		*/
		
		private void Parse(BinaryReader reader)
		{
			DDSStruct header = new DDSStruct();
			Utils.PixelFormat pixelFormat = Utils.PixelFormat.UNKNOWN;
			byte[] data = null;

			if(this.ReadHeader(reader, ref header))
			{
				// patches for stuff
				if(header.depth == 0)
					header.depth = 1;

				uint blocksize = 0;
				pixelFormat = this.GetFormat(header, ref blocksize);
				if(pixelFormat == Utils.PixelFormat.UNKNOWN)
				{
					throw new InvalidFileHeaderException();
				}

				data = this.ReadData(reader, header);
				if(data != null)
				{
					byte[] rawData = Decompressor.Expand(header, data, pixelFormat);
					_bitmap = this.CreateBitmap((int)header.width, (int)header.height, rawData);
				}
			}
		}

		private byte[] ReadData(BinaryReader reader, DDSStruct header)
		{
			byte[] compdata = null;
			uint compsize = 0;

			if((header.flags & Helper.DDSD_LINEARSIZE) > 1)
			{
				compdata = reader.ReadBytes((int)header.sizeorpitch);
				compsize = (uint)compdata.Length;
			}
			else
			{
				uint bps = header.width * header.pixelformat.rgbbitcount / 8;
				compsize = bps * header.height * header.depth;
				compdata = new byte[compsize];

				MemoryStream mem = new MemoryStream((int)compsize);

				byte[] temp;
				for(int z = 0; z < header.depth; z++)
				{
					for(int y = 0; y < header.height; y++)
					{
						temp = reader.ReadBytes((int)bps);
						mem.Write(temp, 0, temp.Length);
					}
				}
				mem.Seek(0, SeekOrigin.Begin);

				mem.Read(compdata, 0, compdata.Length);
				mem.Close();
			}

			return compdata;
		}

		// TODO: Removed alpha, add back as option
		private System.Drawing.Bitmap CreateBitmap(int width, int height, byte[] rawData)
		{
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);// .Format32bppArgb);

			BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height)
				, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);// .Format32bppArgb);
			IntPtr scan = data.Scan0;
			int size = bitmap.Width * bitmap.Height * 4;

			unsafe
			{
				byte* p = (byte*)scan;
				for(int i = 0; i < size; i += 4)
				{
					// iterate through bytes.
					// Bitmap stores it's data in RGBA order.
					// DDS stores it's data in BGRA order.
					p[i] = rawData[i + 2]; // blue
					p[i + 1] = rawData[i + 1]; // green
					p[i + 2] = rawData[i];   // red
					p[i + 3] = rawData[i + 3]; // alpha
				}
			}

			bitmap.UnlockBits(data);
			return bitmap;
		}

		private bool ReadHeader(BinaryReader reader, ref DDSStruct header)
		{
			byte[] signature = reader.ReadBytes(4);
			if(!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
				return false;

			header.size = reader.ReadUInt32();
			if(header.size != 124)
				return false;

			//convert the data
			header.flags = reader.ReadUInt32();
			header.height = reader.ReadUInt32();
			header.width = reader.ReadUInt32();
			header.sizeorpitch = reader.ReadUInt32();
			header.depth = reader.ReadUInt32();
			header.mipmapcount = reader.ReadUInt32();
			header.alphabitdepth = reader.ReadUInt32();

			header.reserved = new uint[10];
			for(int i = 0; i < 10; i++)
			{
				header.reserved[i] = reader.ReadUInt32();
			}

			//pixelfromat
			header.pixelformat.size = reader.ReadUInt32();
			header.pixelformat.flags = reader.ReadUInt32();
			header.pixelformat.fourcc = reader.ReadUInt32();
			header.pixelformat.rgbbitcount = reader.ReadUInt32();
			header.pixelformat.rbitmask = reader.ReadUInt32();
			header.pixelformat.gbitmask = reader.ReadUInt32();
			header.pixelformat.bbitmask = reader.ReadUInt32();
			header.pixelformat.alphabitmask = reader.ReadUInt32();

			//caps
			header.ddscaps.caps1 = reader.ReadUInt32();
			header.ddscaps.caps2 = reader.ReadUInt32();
			header.ddscaps.caps3 = reader.ReadUInt32();
			header.ddscaps.caps4 = reader.ReadUInt32();
			header.texturestage = reader.ReadUInt32();

			return true;
		}

		private Utils.PixelFormat GetFormat(DDSStruct header, ref uint blocksize)
		{
			Utils.PixelFormat format = Utils.PixelFormat.UNKNOWN;
			if((header.pixelformat.flags & Helper.DDPF_FOURCC) == Helper.DDPF_FOURCC)
			{
				blocksize = ((header.width + 3) / 4) * ((header.height + 3) / 4) * header.depth;

				switch(header.pixelformat.fourcc)
				{
				case Helper.FOURCC_DXT1:
					format = Utils.PixelFormat.DXT1;
					blocksize *= 8;
					break;

				case Helper.FOURCC_DXT2:
					format = Utils.PixelFormat.DXT2;
					blocksize *= 16;
					break;

				case Helper.FOURCC_DXT3:
					format = Utils.PixelFormat.DXT3;
					blocksize *= 16;
					break;

				case Helper.FOURCC_DXT4:
					format = Utils.PixelFormat.DXT4;
					blocksize *= 16;
					break;

				case Helper.FOURCC_DXT5:
					format = Utils.PixelFormat.DXT5;
					blocksize *= 16;
					break;

				case Helper.FOURCC_ATI1:
					format = Utils.PixelFormat.ATI1N;
					blocksize *= 8;
					break;

				case Helper.FOURCC_ATI2:
					format = Utils.PixelFormat.THREEDC;
					blocksize *= 16;
					break;

				case Helper.FOURCC_RXGB:
					format = Utils.PixelFormat.RXGB;
					blocksize *= 16;
					break;

				case Helper.FOURCC_DOLLARNULL:
					format = Utils.PixelFormat.A16B16G16R16;
					blocksize = header.width * header.height * header.depth * 8;
					break;

				case Helper.FOURCC_oNULL:
					format = Utils.PixelFormat.R16F;
					blocksize = header.width * header.height * header.depth * 2;
					break;

				case Helper.FOURCC_pNULL:
					format = Utils.PixelFormat.G16R16F;
					blocksize = header.width * header.height * header.depth * 4;
					break;

				case Helper.FOURCC_qNULL:
					format = Utils.PixelFormat.A16B16G16R16F;
					blocksize = header.width * header.height * header.depth * 8;
					break;

				case Helper.FOURCC_rNULL:
					format = Utils.PixelFormat.R32F;
					blocksize = header.width * header.height * header.depth * 4;
					break;

				case Helper.FOURCC_sNULL:
					format = Utils.PixelFormat.G32R32F;
					blocksize = header.width * header.height * header.depth * 8;
					break;

				case Helper.FOURCC_tNULL:
					format = Utils.PixelFormat.A32B32G32R32F;
					blocksize = header.width * header.height * header.depth * 16;
					break;

				default:
					format = Utils.PixelFormat.UNKNOWN;
					blocksize *= 16;
					break;
				} // switch
			}
			else
			{
				// uncompressed image
				if((header.pixelformat.flags & Helper.DDPF_LUMINANCE) == Helper.DDPF_LUMINANCE)
				{
					if((header.pixelformat.flags & Helper.DDPF_ALPHAPIXELS) == Helper.DDPF_ALPHAPIXELS)
					{
						format = Utils.PixelFormat.LUMINANCE_ALPHA;
					}
					else
					{
						format = Utils.PixelFormat.LUMINANCE;
					}
				}
				else
				{
					if((header.pixelformat.flags & Helper.DDPF_ALPHAPIXELS) == Helper.DDPF_ALPHAPIXELS)
					{
						format = Utils.PixelFormat.ARGB;
					}
					else
					{
						format = Utils.PixelFormat.RGB;
					}
				}

				blocksize = (header.width * header.height * header.depth * (header.pixelformat.rgbbitcount >> 3));
			}

			return format;
		}



		// uncomment this to make debugging easier
		/*
		private static String ConvertToHex(byte[] input) {
            String output = "";
            foreach (byte b in input) {
                output = output + String.Format("{0:x2} ", (int)b);

            }
            return output;
        }*/
		
	}
}
