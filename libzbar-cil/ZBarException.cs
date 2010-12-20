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

using System;
using System.Runtime.InteropServices;

namespace ZBar
{
	/// <summary>
	/// An exception that happened in ZBar
	/// </summary>
	public sealed class ZBarException : Exception
	{
		/// <summary>
		/// Verbosity constant, for errors
		/// </summary>
		private const int verbosity = 10;
		
		/// <summary>
		/// Error message
		/// </summary>
		private string message;
		
		/// <summary>
		/// Error code
		/// </summary>
		private ZBarError code;
		
		internal ZBarException(IntPtr obj){
			this.code = (ZBarError)_zbar_get_error_code(obj);
			this.message = Marshal.PtrToStringAnsi(_zbar_error_string(obj, verbosity));
		}
		
		/// <value>
		/// Error message from ZBar
		/// </value>
		public override string Message {
			get {
				return this.message;
			}
		}
		
		/// <value>
		/// Error code of this exception, from ZBar
		/// </value>
		public ZBarError ErrorCode{
			get{
				return this.code;
			}
		}
		
		[DllImport("libzbar")]
		private static extern IntPtr _zbar_error_string(IntPtr obj, int verbosity);
		
		[DllImport("libzbar")]
		private static extern int _zbar_get_error_code(IntPtr obj);
	}
	
	/// <summary>
	/// Error codes
	/// </summary>
	/// <remarks>
	/// The ordering matches zbar_error_t from zbar.h
	/// </remarks>
	public enum ZBarError{
		/// <summary>
		/// No error, or zbar is not aware of the error
		/// </summary>
		Ok = 0,
		
		/// <summary>
		/// Out of memory
		/// </summary>
		OutOfMemory,
		
		/// <summary>
		/// Internal library error
		/// </summary>
		InternalLibraryError,
		
		/// <summary>
		/// Unsupported request
		/// </summary>
		Unsupported,
		
		/// <summary>
		/// Invalid request
		/// </summary>
		InvalidRequest,
		
		/// <summary>
		/// System error
		/// </summary>
		SystemError,
		
		/// <summary>
		/// Locking error
		/// </summary>
		LockingError,
		
		/// <summary>
		/// All resources busy
		/// </summary>
		AllResourcesBusyError,
		
		/// <summary>
		/// X11 display error
		/// </summary>
		X11DisplayError,
		
		/// <summary>
		/// X11 Protocol error
		/// </summary>
		X11ProtocolError,
		
		/// <summary>
		/// Output window closed
		/// </summary>
		OutputWindowClosed,
		
		/// <summary>
		/// Windows system error
		/// </summary>
		WindowsAPIError
	}
}
