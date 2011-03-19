/*------------------------------------------------------------------------
 *  Copyright 2010 (c) Brandon McCaig
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZBar
{
	public static class ZBar
	{
		/// <value>
		/// Get version of the underlying zbar library
		/// </value>
		public static Version Version{
			get{
				uint major = 0;
				uint minor = 0;

				unsafe{
					if(NativeMethods.zbar_version(&major, &minor) != 0){
						throw new Exception("Failed to get ZBar version.");
					}
				}

				return new Version((int)major, (int)minor);
		   }
		}

		#region Extern C Functions.

		private static class NativeMethods{
			[DllImport("libzbar")]
			public static unsafe extern int zbar_version(uint* major, uint* minor);
		}

		#endregion
	}
}
