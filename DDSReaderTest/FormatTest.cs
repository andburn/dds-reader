using System.Drawing;
using System.IO;
using Imaging.DDSReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Tests.DDSReaderTest
{
	// Some simple tests, to check setup
	[TestClass]
	public class FormatTest
	{
		[TestMethod]
		public void DXT1Supported()
		{
			Bitmap bmp = DDS.LoadImage(@"Data\sample_dxt1.dds");
			Assert.IsNotNull(bmp);
		}

		[TestMethod]
		public void DXT3Supported()
		{
			byte[] data = File.ReadAllBytes(@"Data\sample_dxt3.dds");
			Bitmap bmp = DDS.LoadImage(data);
			Assert.IsNotNull(bmp);
		}

		[TestMethod]
		public void DXT5Supported()
		{
			Stream stream = File.Open(@"Data\sample_dxt5.dds", FileMode.Open);
			Bitmap bmp = DDS.LoadImage(stream);
			Assert.IsNotNull(bmp);
		}

		[TestMethod]
		public void PreserveAlphaChannel_DefaultsTrue()
		{
			DDSImage im = new DDSImage(File.Open(@"Data\sample_dxt5.dds", FileMode.Open));
			Assert.IsTrue(im.PreserveAlpha);
		}

		[TestMethod]
		public void PreserveAlphaChannel_IsFalse()
		{
			DDSImage im = new DDSImage(File.Open(@"Data\sample_dxt5.dds", FileMode.Open), false);
			Assert.IsFalse(im.PreserveAlpha);
		}

		[TestMethod]
		public void PNGNotSupported()
		{
			Bitmap bmp = DDS.LoadImage(@"Data\sample.png");
			Assert.IsNull(bmp);
		}

		[TestMethod]
		public void BMPNotSupported()
		{
			Bitmap bmp = DDS.LoadImage(@"Data\sample.bmp");
			Assert.IsNull(bmp);
		}
	}
}