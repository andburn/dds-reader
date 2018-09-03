using System;

namespace DDSReader.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var dds = new DDSImage(args[0]);
			dds.Save(args[1]);
		}
	}
}
