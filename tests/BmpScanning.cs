/*------------------------------------------------------------------------
 *  Copyright 2010 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
 * 
 *  This file is part of the ZBar CIL Wrapper.
 *
 *  The ZBar CIL Wrapper is free software; you can redistribute it
 *  and/or modify it under the terms of the GNU Lesser Public License as
 *  published by the Free Software Foundation; either version 2.1 of
 *  the License, or (at your option) any later version.
 *
 *  The ZBar CIL Wrapper is distributed in the hope that it will be
 *  useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 *  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser Public License
 *  along with the ZBar CIL Wrapper; if not, write to the Free
 *  Software Foundation, Inc., 51 Franklin St, Fifth Floor,
 *  Boston, MA  02110-1301  USA
 * 
 *------------------------------------------------------------------------*/

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
