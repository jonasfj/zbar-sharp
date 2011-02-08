/*------------------------------------------------------------------------
 *  Copyright 2009 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
 *  Copyright 2007-2009 (c) Jeff Brown <spadix@users.sourceforge.net>
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
using System.Runtime.InteropServices;

namespace ZBar
{
	/// <summary>
	/// Representation of a decoded symbol
	/// </summary>
	/// <remarks>This symbol does not hold any references to unmanaged resources.</remarks>
	public class Symbol
	{
		/// <summary>
		/// Initialize a symbol from pointer to a symbol
		/// </summary>
		/// <param name="symbol">
		/// Pointer to a symbol
		/// </param>
		internal Symbol(IntPtr symbol){
			if(symbol == IntPtr.Zero)
				throw new Exception("Can't initialize symbol from null pointer.");
			
			//Get data from the symbol
			IntPtr pData = zbar_symbol_get_data(symbol);
			int length = (int)zbar_symbol_get_data_length(symbol);
			this.data = Marshal.PtrToStringAnsi(pData, length);
			
			//Get the other fields
			this.type = (SymbolType)zbar_symbol_get_type(symbol);
			this.quality = zbar_symbol_get_quality(symbol);
			this.count = zbar_symbol_get_count(symbol);
		}
		
		private string data;
		private int quality;
		private int count;
		private SymbolType type;

		public override string ToString(){
			return this.type.ToString() + " " + this.data;
		}
		
		#region Public properties
		
		/// <value>
		/// Retrieve current cache count.
		/// </value>
		/// <remarks>
		/// When the cache is enabled for the image_scanner this provides inter-frame reliability and redundancy information for video streams. 
		/// 	&lt; 0 if symbol is still uncertain.
		/// 	0 if symbol is newly verified.
		/// 	&gt; 0 for duplicate symbols 
		/// </remarks>
		public int Count{
			get{
				return this.count;
			}
		}

		/// <value>
		/// Data decoded from symbol.
		/// </value>
		public string Data{
			get{
				return this.data;
			}
		}

		/// <value>
		/// Get a symbol confidence metric. 
		/// </value>
		/// <remarks>
		/// An unscaled, relative quantity: larger values are better than smaller values, where "large" and "small" are application dependent.
		/// </remarks>
		public int Quality{
			get{
				return this.quality;
			}
		}

		/// <value>
		/// Type of decoded symbol
		/// </value>
		public SymbolType Type{
			get{
				return this.type;
			}
		}
		
		#endregion
		
		#region Extern C functions
		/// <summary>
		/// symbol reference count manipulation.
		/// </summary>
        /// <remarks>
		/// increment the reference count when you store a new reference to the
		/// symbol.  decrement when the reference is no longer used.  do not
		/// refer to the symbol once the count is decremented and the
		/// containing image has been recycled or destroyed.
		/// the containing image holds a reference to the symbol, so you
		/// only need to use this if you keep a symbol after the image has been
		/// destroyed or reused.
		/// </remarks>
		[DllImport("libzbar")]
		private static extern void zbar_symbol_ref(IntPtr symbol, int refs);
		
		/// <summary>
		/// retrieve type of decoded symbol.
		/// </summary>
		/// <returns> the symbol type</returns>
		[DllImport("libzbar")]
		private static extern int zbar_symbol_get_type(IntPtr symbol);
		
		/// <summary>
		/// retrieve data decoded from symbol.
		/// </summary>
		/// <returns> the data string</returns>
		[DllImport("libzbar")]
		private static extern IntPtr zbar_symbol_get_data(IntPtr symbol);
		
		/// <summary>
		/// retrieve length of binary data.
		/// </summary>
		/// <returns> the length of the decoded data</returns>
		[DllImport("libzbar")]
		private static extern uint zbar_symbol_get_data_length(IntPtr symbol);
		
		/// <summary>
		/// retrieve a symbol confidence metric.
		/// </summary>
		/// <returns> an unscaled, relative quantity: larger values are better
		/// than smaller values, where "large" and "small" are application
		/// dependent.
        /// </returns>
		/// <remarks>expect the exact definition of this quantity to change as the
		/// metric is refined.  currently, only the ordered relationship
		/// between two values is defined and will remain stable in the future
        /// </remarks>
		[DllImport("libzbar")]
		private static extern int zbar_symbol_get_quality(IntPtr symbol);
		
		/// <summary>
		/// retrieve current cache count.
		/// </summary>
        /// <remarks>when the cache is enabled for the
		/// image_scanner this provides inter-frame reliability and redundancy
		/// information for video streams.
        /// </remarks>
		/// <returns>
        /// < 0 if symbol is still uncertain.
		/// 0 if symbol is newly verified.
		/// > 0 for duplicate symbols
        /// </returns>
		[DllImport("libzbar")]
		private static extern int zbar_symbol_get_count(IntPtr symbol);
		
		/// <summary>
		/// retrieve the number of points in the location polygon.  the
		/// location polygon defines the image area that the symbol was
		/// extracted from.
		/// </summary>
		/// <returns> the number of points in the location polygon</returns>
		/// <remarks>this is currently not a polygon, but the scan locations
		/// where the symbol was decoded</remarks>
		[DllImport("libzbar")]
		private static extern uint zbar_symbol_get_loc_size(IntPtr symbol);
		
		/// <summary>
		/// retrieve location polygon x-coordinates.
		/// points are specified by 0-based index.
		/// </summary>
		/// <returns> the x-coordinate for a point in the location polygon.
		/// -1 if index is out of range</returns>
		[DllImport("libzbar")]
		private static extern int zbar_symbol_get_loc_x(IntPtr symbol, uint index);
		
		/// <summary>
		/// retrieve location polygon y-coordinates.
		/// points are specified by 0-based index.
		/// </summary>
		/// <returns> the y-coordinate for a point in the location polygon.
		///  -1 if index is out of range</returns>
		[DllImport("libzbar")]
		private static extern int zbar_symbol_get_loc_y(IntPtr symbol, uint index);
		
		/// <summary>
		/// iterate the result set.
		/// </summary>
		/// <returns> the next result symbol, or
		/// NULL when no more results are available</returns>
		/// <remarks>Marked internal because it is used by the symbol iterators.</remarks>
		[DllImport("libzbar")]
		internal static extern IntPtr zbar_symbol_next(IntPtr symbol);
		
		/// <summary>
		/// print XML symbol element representation to user result buffer.
		/// </summary>
		/// <remarks>see http://zbar.sourceforge.net/2008/barcode.xsd for the schema.</remarks>
		/// <param name="symbol">is the symbol to print</param>
		/// <param name="buffer"> is the inout result pointer, it will be reallocated
		/// with a larger size if necessary.</param>
		/// <param name="buflen">  is inout length of the result buffer.</param>
		/// <returns> the buffer pointer</returns>
		[DllImport("libzbar")]
		private static extern IntPtr zbar_symbol_xml(IntPtr symbol, out IntPtr buffer, out uint buflen);
		#endregion
	}
	
	/// <summary>
	/// Different symbol types
	/// </summary>
	[Flags]
	public enum SymbolType{
		/// <summary>
		/// No symbol decoded
		/// </summary>
		None 	= 0,
		
		/// <summary>
		/// Intermediate status
		/// </summary>
		Partial	= 1,
		
		/// <summary>
		/// EAN-8
		/// </summary>
		EAN8	= 8,
		
		/// <summary>
		/// UPC-E
		/// </summary>
		UPCE	= 9,
		
		/// <summary>
		/// ISBN-10 (from EAN-13)
		/// </summary>
		ISBN10	= 10,
		
		/// <summary>
		/// UPC-A
		/// </summary>
		UPCA	= 12,
		
		/// <summary>
		/// EAN-13
		/// </summary>
		EAN13	= 13,
		
		/// <summary>
		/// ISBN-13 (from EAN-13)
		/// </summary>
		ISBN13	= 14,
		
		/// <summary>
		/// Interleaved 2 of 5.
		/// </summary>
		I25		= 25,
		
		/// <summary>
		/// Code 39.
		/// </summary>
		CODE39	= 39,
		
		/// <summary>
		/// PDF417
		/// </summary>
		PDF417	= 57,
		
		/// <summary>
		/// QR Code
		/// </summary>
		QRCODE  = 64,
		
		/// <summary>
		/// Code 128
		/// </summary>
		CODE128 = 128,
		
		/// <summary>
		/// mask for base symbol type
		/// </summary>
		Symbole = 0x00ff,
		
		/// <summary>
		/// 2-digit add-on flag
		/// </summary>
		Addon2 	= 0x0200,
		
		/// <summary>
		/// 5-digit add-on flag
		/// </summary>
		Addon5 	= 0x0500,
		
		/// <summary>
		/// add-on flag mask
		/// </summary>
		Addon	= 0x0700
	}
}
