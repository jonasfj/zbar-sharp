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

/// <summary>
/// ZBar is a library for reading bar codes from video streams
/// </summary>
namespace ZBar
{
	/// <summary>
	/// Mid-level video source abstraction. captures images from a video device 
	/// </summary>
	public class Video : IDisposable
	{
		private IntPtr video = IntPtr.Zero;
		private bool enabled = false;
		
		/// <summary>
		/// Create a video instance
		/// </summary>
		public Video(){
			this.video = zbar_video_create();
			if(this.video == IntPtr.Zero)
				throw new Exception("Didn't create an unmanaged Video instance, don't know what happened.");
		}
		
		#region Wrapper methods
		
		/// <summary>
		/// Open and probe a video device. 
		/// </summary>
		/// <param name="device">
		/// The device specified by platform specific unique name (v4l device node path in *nix eg "/dev/video", DirectShow DevicePath property in windows). 
		/// </param>
		public void Open(string device){
			if(zbar_video_open(this.video, device) != 0){
				throw new ZBarException(this.video);
			}
		}
		
		/// <summary>
		/// Close the video device
		/// </summary>
		public void Close(){
			if(zbar_video_open(this.video, null) != 0){
				throw new ZBarException(this.video);
			}
		}
		
		/// <value>
		/// Start/stop video capture, must be called after Open()
		/// </value>
		public bool Enabled{
			set{
				if(zbar_video_enable(this.video, value ? 1 : 0) != 0)
					throw new ZBarException(this.video);
				this.enabled = value;
			}
			get{
				return this.enabled;
			}
		}
		
		/// <value>
		/// Get output handle width
		/// </value>
		public int Width{
			get{
				int width = zbar_video_get_width(this.video);
				if(width == 0)
					throw new Exception("Video device not opened!");
				return width;
			}
		}
		
		/// <value>
		/// Get output image height
		/// </value>
		public int Height{
			get{
				int height = zbar_video_get_height(this.video);
				if(height == 0)
					throw new Exception("Video device not opened!");
				return height;
			}
		}
		
		/// <summary>
		/// Request a other output image size
		/// </summary>
		/// <remarks>
		/// The request may be adjusted or completely ignored by the driver.
		/// </remarks>
		/// <param name="width">
		/// Desired output width
		/// </param>
		/// <param name="height">
		/// Desired output height
		/// </param>
		public void RequestSize(uint width, uint height){
			if(zbar_video_request_size(this.video, width, height) != 0)
				throw new ZBarException(this.video);
		}
		
		/// <summary>
		/// Retrieve next captured image
		/// </summary>
		/// <remarks>This method blocks untill an image have been captured.</remarks>
		/// <returns>
		/// A <see cref="Image"/> representating the next image captured
		/// </returns>
		public Image NextFrame(){
			IntPtr image = zbar_video_next_image(this.video);
			if(image == IntPtr.Zero)
				throw new ZBarException(this.video);
			return new Image(image, false); //I don't think we need to increment reference count here..
		}
		
		#endregion
		
		#region IDisposable Implementation
		//This pattern for implementing IDisposable is recommended by:
		//Framework Design Guidelines, 2. Edition, Section 9.4
		
		/// <summary>
		/// Dispose this object
		/// </summary>
		/// <remarks>
		/// This boolean disposing parameter here ensures that objects with a finalizer is not disposed,
		/// this is method is invoked from the finalizer. Do overwrite, and call, this method in base 
		/// classes if you use any unmanaged resources.
		/// </remarks>
		/// <param name="disposing">
		/// A <see cref="System.Boolean"/> False if called from the finalizer, True if called from Dispose.
		/// </param>
		protected virtual void Dispose(bool disposing){
			if(this.video != IntPtr.Zero){
				zbar_video_destroy(this.video);
				this.video = IntPtr.Zero;
			}
			if(disposing){
				//Release finalizable resources, at the moment none.
			}
		}
		
		/// <summary>
		/// Release resources held by this object
		/// </summary>
		public void Dispose(){
			//We're disposing this object and can release objects that are finalizable
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		/// <summary>
		/// Finalize this object
		/// </summary>
		~Video(){
			//Dispose this object, but do NOT release finalizable objects, we don't know in which order
			//these are release and they may already be finalized.
			this.Dispose(false);
		}
		#endregion
		
		#region Extern C functions
		
		/// <summary> constructor. </summary>
		[DllImport("libzbar")]
		private static extern IntPtr zbar_video_create();
		
		/// <summary> destructor. </summary>
		[DllImport("libzbar")]
		private static extern void zbar_video_destroy(IntPtr video);
		
		/// <summary>
		/// open and probe a video device.
		/// the device specified by platform specific unique name
		/// (v4l device node path in *nix eg "/dev/video",
		///  DirectShow DevicePath property in windows).
		/// </summary>
		/// <returns> 0 if successful or -1 if an error occurs</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_open(IntPtr video, string device);
		
		/// <summary>
		/// retrieve file descriptor associated with open *nix video device
		/// useful for using select()/poll() to tell when new images are
		/// available (NB v4l2 only!!).
		/// </summary>
		/// <returns> the file descriptor or -1 if the video device is not open
		/// or the driver only supports v4l1</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_get_fd(IntPtr video);
		
		/// <summary>
		/// request a preferred size for the video image from the device.
		/// the request may be adjusted or completely ignored by the driver.
		/// </summary>
		/// <returns> 0 if successful or -1 if the video device is already
		/// initialized</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_request_size(IntPtr video, uint width, uint height);
		
		/// <summary>
		/// request a preferred driver interface version for debug/testing.
		/// </summary>
		[DllImport("libzbar")]
		private static extern int zbar_video_request_interface(IntPtr video, int version);
		
		/// <summary>
		/// request a preferred I/O mode for debug/testing.
		/// </summary>
        /// <remarks>You will get
		/// errors if the driver does not support the specified mode.
		/// @verbatim
		///     0 = auto-detect
		///     1 = force I/O using read()
		///     2 = force memory mapped I/O using mmap()
		///     3 = force USERPTR I/O (v4l2 only)
		/// @endverbatim
		/// must be called before zbar_video_open()
        /// </remarks>
		[DllImport("libzbar")]
		private static extern int zbar_video_request_iomode(IntPtr video, int iomode);
		
		/// <summary>
		/// retrieve current output image width.
		/// </summary>
		/// <returns>the width or 0 if the video device is not open</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_get_width(IntPtr video);
		
		/// <summary>
		/// retrieve current output image height.
		/// </summary>
		/// <returns>the height or 0 if the video device is not open</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_get_height(IntPtr video);
		
		/// <summary>
		/// initialize video using a specific format for debug.
		/// use zbar_negotiate_format() to automatically select and initialize
		/// the best available format
		/// </summary>
		[DllImport("libzbar")]
		private static extern int zbar_video_init(IntPtr video, uint format);
		
		/// <summary>
		/// start/stop video capture.
		/// all buffered images are retired when capture is disabled.
		/// </summary>
		/// <returns> 0 if successful or -1 if an error occurs</returns>
		[DllImport("libzbar")]
		private static extern int zbar_video_enable(IntPtr video, int enable);
		
		/// <summary>
		/// retrieve next captured image.  blocks until an image is available.
		/// </summary>
		/// <returns> NULL if video is not enabled or an error occurs</returns>
		[DllImport("libzbar")]
		private static extern IntPtr zbar_video_next_image(IntPtr video);
		
		#endregion
	}
}
