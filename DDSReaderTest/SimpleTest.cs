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
		public void BlackBox()
		{
			byte[] data = File.ReadAllBytes(@"E:\Dump\sample.dds");
			
			Bitmap bmp = DDSReader.DDSReader.LoadImage(data);

			bmp.Save(@"E:\Dump\dds_sample.png");

			Assert.Fail();
		}
	}
}
