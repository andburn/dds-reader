using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace DDSReader
{
	public class DDSImage
	{
		private readonly Pfim.IImage _image;

		public byte[] Data
		{
			get
			{
				if (_image != null)
					return _image.Data;
				else
					return new byte[0];
			}
		}

		public DDSImage(string file)
		{
			_image = Pfim.Pfim.FromFile(file);
			Decompress();
		}

		public DDSImage(Stream stream)
		{
			if (stream == null)
				throw new Exception("Null Stream");

			_image = Pfim.Dds.Create(stream, new Pfim.PfimConfig());
			Decompress();
		}

		public DDSImage(byte[] data)
		{
			if (data == null || data.Length <= 0)
				throw new Exception("Empty Data");

			_image = Pfim.Dds.Create(data, new Pfim.PfimConfig());
			Decompress();
		}

		public void Save(string file)
		{
			if (_image.Format == Pfim.ImageFormat.Rgba32)
				Save<Rgba32>(file);
			else if (_image.Format == Pfim.ImageFormat.Rgb24)
				Save<Rgb24>(file);
			else
				throw new Exception("Unsupported pixel format");
		}

		private void Decompress()
		{
			if (_image != null && _image.Compressed)
				_image.Decompress();
		}

		private void Save<T>(string file)
			where T : struct, IPixel<T>
		{
			Image<T> image = Image.LoadPixelData<T>(
				_image.Data, _image.Width, _image.Height);
			image.Save(file);
		}

	}
}
