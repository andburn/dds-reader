/*
 * DDSReader
 * Copyright 2006 Michael Farrell 
 * LGPLv2.1
 */
using System;

namespace me.andburn.DDSReader
{
	public class Utils
	{
		private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
		{
			int temp;

			temp = (color >> 11) * 255 + 16;
			r = (byte)((temp / 32 + temp) / 32);
			temp = ((color & 0x07E0) >> 5) * 255 + 32;
			g = (byte)((temp / 64 + temp) / 64);
			temp = (color & 0x001F) * 255 + 16;
			b = (byte)((temp / 32 + temp) / 32);
		}
	}

	struct Colour8888
	{
		public byte r;
		public byte g;
		public byte b;
		public byte a;
	}

	struct Colour565
	{
		public ushort blue; //: 5;
		public ushort green; //: 6;
		public ushort red; //: 5;
	}

	/// <summary>
	/// Various pixel formats/compressors used by the DDS image.
	/// </summary>
	enum PixelFormat
	{
		/// <summary>
		/// 32-bit image, with 8-bit red, green, blue and alpha.
		/// </summary>
		ARGB,
		/// <summary>
		/// 24-bit image with 8-bit red, green, blue.
		/// </summary>
		RGB,
		/// <summary>
		/// 16-bit DXT-1 compression, 1-bit alpha.
		/// </summary>
		DXT1,
		/// <summary>
		/// DXT-2 Compression
		/// </summary>
		DXT2,
		/// <summary>
		/// DXT-3 Compression
		/// </summary>
		DXT3,
		/// <summary>
		/// DXT-4 Compression
		/// </summary>
		DXT4,
		/// <summary>
		/// DXT-5 Compression
		/// </summary>
		DTX5,
		/// <summary>
		/// 3DC Compression
		/// </summary>
		THREEDC,
		/// <summary>
		/// ATI1n Compression
		/// </summary>
		ATI1N,
		LUMINANCE,
		LUMINANCE_ALPHA,
		RXGB,
		A16B16G16R16,
		R16F,
		G16R16F,
		A16B16G16R16F,
		R32F,
		G32R32F,
		A32B32G32R32F,
		/// <summary>
		/// Unknown pixel format.
		/// </summary>
		UNKNOWN
	}
}
