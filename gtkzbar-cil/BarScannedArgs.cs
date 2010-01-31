/*------------------------------------------------------------------------
 *  Copyright 2009 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
 *
 *  This file is part of the Gtk# ZBar widget
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

using ZBar;
using System;

namespace GtkZBar
{
	/// <summary>
	/// BarScanned event arguments
	/// </summary>
	public sealed class BarScannedArgs : EventArgs
	{
		private Symbol symbol = null;
		
		internal BarScannedArgs(Symbol symbol)
		{
			this.symbol = symbol;
		}
	
		/// <value>
		/// The symbol that was scanned
		/// </value>
		public Symbol Symbol{
			get{
				return this.symbol;
			}
		}
	}
}
