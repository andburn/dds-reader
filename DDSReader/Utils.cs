/*
 * DDSReader
 * Copyright 2006 Michael Farrell 
 * LGPLv2.1
 */
using System;
using System.Runtime.InteropServices;

namespace me.andburn.DDSReader
{
	public class Utils
	{
		#region Constants
		// DDSStruct flags
		public const int DDSD_CAPS = 0x00000001;
		public const int DDSD_HEIGHT = 0x00000002;
		public const int DDSD_WIDTH = 0x00000004;
		public const int DDSD_PITCH = 0x00000008;
		public const int DDSD_PIXELFORMAT = 0x00001000;
		public const int DDSD_MIPMAPCOUNT = 0x00020000;
		public const int DDSD_LINEARSIZE = 0x00080000;
		public const int DDSD_DEPTH = 0x00800000;
		// PixelFormat values
		public const int DDPF_ALPHAPIXELS = 0x00000001;
		public const int DDPF_FOURCC = 0x00000004;
		public const int DDPF_RGB = 0x00000040;
		public const int DDPF_LUMINANCE = 0x00020000;
		// DDSCaps
		public const int DDSCAPS_COMPLEX = 0x00000008;
		public const int DDSCAPS_TEXTURE = 0x00001000;
		public const int DDSCAPS_MIPMAP = 0x00400000;
		public const int DDSCAPS2_CUBEMAP = 0x00000200;
		public const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
		public const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
		public const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
		public const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
		public const int DDSCAPS2_VOLUME = 0x00200000;
		// FOURCC
		public const uint FOURCC_DXT1 = 0x31545844;
		public const uint FOURCC_DXT2 = 0x32545844;
		public const uint FOURCC_DXT3 = 0x33545844;
		public const uint FOURCC_DXT4 = 0x34545844;
		public const uint FOURCC_DXT5 = 0x35545844;
		public const uint FOURCC_ATI1 = 0x31495441;
		public const uint FOURCC_ATI2 = 0x32495441;
		public const uint FOURCC_RXGB = 0x42475852;
		public const uint FOURCC_DOLLARNULL = 0x24;
		public const uint FOURCC_oNULL = 0x6f;
		public const uint FOURCC_pNULL = 0x70;
		public const uint FOURCC_qNULL = 0x71;
		public const uint FOURCC_rNULL = 0x72;
		public const uint FOURCC_sNULL = 0x73;
		public const uint FOURCC_tNULL = 0x74;
		#endregion

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

	[StructLayout(LayoutKind.Sequential)]
	struct Colour8888
	{
		public byte red;
		public byte green;
		public byte blue;
		public byte alpha;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	struct Colour565
	{
		public ushort blue; //: 5;
		public ushort green; //: 6;
		public ushort red; //: 5;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct DDSStruct
	{
		public uint size;		// equals size of struct (which is part of the data file!)
		public uint flags;
		public uint height;
		public uint width;
		public uint sizeorpitch;
		public uint depth;
		public uint mipmapcount;
		public uint alphabitdepth;
		//[MarshalAs(UnmanagedType.U4, SizeConst = 11)]
		public uint[] reserved;//[11];

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct pixelformatstruct
		{
			public uint size;	// equals size of struct (which is part of the data file!)
			public uint flags;
			public uint fourcc;
			public uint rgbbitcount;
			public uint rbitmask;
			public uint gbitmask;
			public uint bbitmask;
			public uint alphabitmask;
		}
		public pixelformatstruct pixelformat;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct ddscapsstruct
		{
			public uint caps1;
			public uint caps2;
			public uint caps3;
			public uint caps4;
		}
		public ddscapsstruct ddscaps;
		public uint texturestage;

		//#ifndef __i386__
		//void to_little_endian()
		//{
		//	size_t size = sizeof(DDSStruct);
		//	assert(size % 4 == 0);
		//	size /= 4;
		//	for (size_t i=0; i<size; i++)
		//	{
		//		((int32_t*) this)[i] = little_endian(((int32_t*) this)[i]);
		//	}
		//}
		//#endif
	}

	enum PixelFormat
	{
		ARGB,
		RGB,
		DXT1,
		DXT2,
		DXT3,
		DXT4,
		DXT5,
		THREEDC,
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
		UNKNOWN
	}
}
