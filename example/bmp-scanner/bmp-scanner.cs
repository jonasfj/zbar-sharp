/*------------------------------------------------------------------------
 *  Copyright 2009 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
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

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZBar;

namespace BMPScanner
{
	/// <summary>
	/// Simple example program that scans images given as argument 
	/// </summary>
	/// <remarks>If no argument is provided, it scans a file called barcode.bmp</remarks>
	class Program
	{
		static void Main(string[] args){
			//List of files to scan
			List<string> files = new List<string>();
			
			//Copy arguments into files (ignore this part)
			for(int i = 1; i < args.Length; i++){
				if(args[i] == "--help" || args[i] == "-h")
					Console.WriteLine("Usage ./{0} [file1] [file2] [file3] ...", args[0]);
				else
					files.Add(args[i]);
			}
			if(files.Count == 0)
				files.Add("barcode.bmp");
			
			//Create an instance of scanner
			using(ImageScanner scanner = new ImageScanner()){
				//We won't use caching here
				scanner.Cache = false;
				
				//For each file that we need scanned
				foreach(string file in files){
					Console.WriteLine("Symbols in {0}:", file);
					
					//Open the file, using System.Drawing to read it
					System.Drawing.Image img = System.Drawing.Image.FromFile(file);
					
					//Scan the image for symbols, using System.Drawing and ZBar for conversation
					//Please note that this is no way an efficient implementation, more optimizations
					//of the conversation code etc, could easily be implemented.
					List<Symbol> symbols = scanner.Scan(img);
					
					//For each symbol we've found
					foreach(Symbol symbol in symbols)
						Console.WriteLine("\t" + symbol.ToString());
				}
			}
		}
	}
}
