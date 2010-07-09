using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZBar;

namespace BMPScanner
{
	class Program
	{
		static void Main(string[] args) {
			//Don't care to read the header now... it takes time :)
			//So I'm just hardcoding size...
			uint width = 1200;
			uint height = 775;
			byte[] data = new byte[width * height * 3];
			//Read the file
			using(FileStream fs = new FileStream("barcode.bmp", FileMode.Open)) {
				//Skip the header
				fs.Seek(54, SeekOrigin.Begin);
				fs.Read(data, 0, data.Length);
			}

            //Create an empty image instance
			Image img = new Image();
			img.Data = data; //Set the data property
			img.Width = width; //Set width and height
			img.Height = height;
			img.Format = 0x33424752; //Trying with RGB3, as I know it's 24bit BMP
			Image grey = img.Convert(0x30303859); //Convert to GREY/Y800
			
            //Create a scanner
			using(ImageScanner scanner = new ImageScanner()) {
				scanner.Cache = false;
				scanner.Scan(grey); //Scan the image
			}

			Console.WriteLine("Symboles: ");
            //Now enumerate over the symboles found... These have been associated with grey
			foreach(Symbol sym in grey.Symbols) {
				Console.WriteLine(sym.ToString());
			}
			Console.WriteLine("-End of program-");
            Console.ReadLine();
		}
	}
}
