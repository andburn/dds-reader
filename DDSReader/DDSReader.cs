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
using System.Drawing;

namespace au.id.micolous.libs.DDSReader
{
    /// <summary>
    /// This is the main class of the library.  All static methods are contained within.
    /// </summary>
    public static class DDSReader
    {

        /// <summary>
        /// Loads a DDS image file, and returns a Bitmap object of the image.
        /// </summary>
        /// <param name="data">The image data.</param>
        /// <returns>The Bitmap representation of the image.</returns>
        public static Bitmap LoadImage(byte[] data)
        {
        	DDSImage im = new DDSImage(data);
        	return im.BitmapImage;
        }
        
        /// <summary>
        /// "Pings" the library.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public static bool Ping()
        {
            return true;
        }
       
    }
}
