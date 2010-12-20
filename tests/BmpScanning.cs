using System;
using NUnit.Framework;
using ZBar;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace tests
{
	/// <summary>
	/// Simple test of BMP conversation and scanning
	/// </summary>
	[TestFixture()]
	public class BmpScanning
	{
		[Test()]
		public void ConvertAndScanBMP(){
			System.Drawing.Image img = System.Drawing.Image.FromFile("images/barcode.bmp");
			
			ImageScanner scanner = new ImageScanner();
			List<Symbol> symbols = scanner.Scan(img);
			
			Assert.AreEqual(1, symbols.Count, "Didn't find the symbols");
			Assert.IsTrue(SymbolType.EAN13 == symbols[0].Type, "Didn't get the barcode type right");
			Assert.AreEqual("0123456789012", symbols[0].Data, "Didn't read the correct barcode");
		}
	}
}
