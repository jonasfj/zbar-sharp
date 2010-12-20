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
