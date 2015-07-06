/*
 * DDSReader
 * Copyright 2006 Michael Farrell 
 * LGPLv2.1
 */
using System.Drawing;

namespace me.andburn.DDSReader
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
