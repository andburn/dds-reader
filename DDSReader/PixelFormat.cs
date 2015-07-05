/*
 * DDSReader
 * Copyright 2006 Michael Farrell
 
    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
namespace au.id.micolous.libs.DDSReader
{
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
