using System.Drawing;
using System.IO;
using AndBurn.DDSReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndBurn.DDSReaderTest
{
    // Some simple tests, to check setup
    [TestClass]
    public class FormatTest
    {
        [TestMethod]
        public void DXT1Supported()
        {
            Bitmap bmp = DDS.LoadImage(@"Data\DXT1.dds");
            Assert.IsNotNull(bmp);
        }

        [TestMethod]
        public void DXT3Supported()
        {
            byte[] data = File.ReadAllBytes(@"Data\DXT3.dds");
            Bitmap bmp = DDS.LoadImage(data);
            Assert.IsNotNull(bmp);
        }

        [TestMethod]
        public void DXT5Supported()
        {
            Stream stream = File.Open(@"Data\DXT5.dds", FileMode.Open);
            Bitmap bmp = DDS.LoadImage(stream);
            Assert.IsNotNull(bmp);
        }

        [TestMethod]
        public void PNGNotSupported()
        {
            Bitmap bmp = DDS.LoadImage(@"Data\CRED.png");
            Assert.IsNull(bmp);
        }
    }
}