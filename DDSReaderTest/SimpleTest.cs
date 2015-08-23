using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using me.andburn.DDSReader;
using System.IO;
using System.Drawing;

namespace me.andburn.DDSReaderTest
{
	[TestClass]
	public class SimpleTest
	{
		[TestMethod]
		[ExpectedException(typeof(UnknownFileFormatException))]
		public void DXT5NotSupported()
		{
			byte[] data = File.ReadAllBytes(@"..\..\..\Data\DXT5.dds");
			
			Bitmap bmp = DDSReader.DDSReader.LoadImage(data);
			
			// Assert - to be an exception
		}

		[TestMethod]
		public void DXT1Supported()
		{
			byte[] data = File.ReadAllBytes(@"..\..\..\Data\DXT1.dds");

			Bitmap bmp = DDSReader.DDSReader.LoadImage(data);

			Assert.IsFalse(bmp.Size.IsEmpty);
		}
	}
}
