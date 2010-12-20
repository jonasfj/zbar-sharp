using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZBar
{
	/// <summary>
	/// High-level self-contained image processor
	/// </summary>
	public sealed class Processor : IDisposable
	{
		/// <summary>
		/// Verbosity constant, for errors
		/// </summary>
		private const int verbosity = 10;
		
		/// <summary>
		/// Pointer to unmanaged processor
		/// </summary>
		private IntPtr _processor;

		/// <summary>
		/// Create processor
		/// </summary>
		/// <param name="threaded">
		/// if threaded is set and threading is available the processor
		/// will spawn threads where appropriate to avoid blocking and
		/// improve responsiveness
		/// </param>
		public Processor(bool threaded){
			this._processor = NativeMethods.zbar_processor_create(threaded ? 1 : 0);

			if(this._processor == IntPtr.Zero)
				throw new Exception("Couldn't create unmanaged processor. Reason unknown.");
		}

		/// <summary>
		/// Create processor with an initial size
		/// </summary>
		/// <param name="threaded">
		/// if threaded is set and threading is available the processor
		/// will spawn threads where appropriate to avoid blocking and
		/// improve responsiveness
		/// </param>
		public Processor(bool threaded, uint width, uint height) : this(threaded){
			this.RequestSize(width, height);
		}

		/// <summary>
		/// Release allocated resources
		/// </summary>
		private void Destroy(){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot destroy. Unmanaged processor is null.");

			NativeMethods.zbar_processor_set_active(this._processor, 0);
			NativeMethods.zbar_processor_destroy(this._processor);

			this._processor = IntPtr.Zero;
		}
		
		public event EventHandler<ImageDataEventArgs> ImageData;

		/// <remarks>
		/// No clue how to factor the user data into things safely mixing
		/// managed types with pointers. For now, we'll just skip that.
		/// </remarks>
		private void ImageDataHandler(IntPtr imagePtr){
			if(imagePtr == IntPtr.Zero)
				throw new ArgumentNullException("imagePtr");
			
			this.OnImageData(new ImageDataEventArgs(new Image(imagePtr, true), null));
		}
		
		private void OnImageData(ImageDataEventArgs e){
			if(this.ImageData != null)
				this.ImageData(this, e);
		}

		/// <summary>
		/// opens a video input device and/or prepares to display output
		/// </summary>
		/// <param name="video">
		/// Video device to open
		/// </param>
		public void Init(string video, bool enableDisplay){
			NativeMethods.zbar_processor_init(this._processor, video, enableDisplay ? 1 : 0);
		}

		public void ParseConfig(string configString){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot parse config. Unmanaged processor is null.");

			int result = NativeMethods.zbar_processor_parse_config(this._processor, configString);

			if(result != 0)
				throw new ZBarException(this._processor);
		}

		/// <summary>
		/// request a preferred size for the video image from the device.
		/// the request may be adjusted or completely ignored by the driver.
		/// </summary>
		public void RequestSize(uint width, uint height){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot request size. Unmanaged processor is null.");

			int result = NativeMethods.zbar_processor_request_size(this._processor, width, height);

			if(result != 0)
				throw new ZBarException(this._processor);
		}

		/// <summary>
		/// control the processor in free running video mode.
		/// only works if video input is initialized. if threading is in use,
		/// scanning will occur in the background, otherwise this is only
		/// useful wrapping calls to zbar_processor_user_wait(). if the
		/// library output window is visible, video display will be enabled.
		/// </summary>
		/// <param name="active">
		/// A <see cref="System.Boolean"/>
		/// </param>
		public void SetActive(bool active){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot set active. Unmanaged processor is null.");

			var result = NativeMethods.zbar_processor_set_active(this._processor, active ? 1 : 0);

			if(result != 0)
				throw new ZBarException(this._processor);
		}

		public void SetConfig(SymbolType symbology, Config config, int value){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot set config. Unmanaged processor is null.");

			int result = NativeMethods.zbar_processor_set_config(this._processor, (int)symbology, (int)config, value);

			if(result != 0)
				throw new ZBarException(this._processor);
		}

		private IntPtr SetDataHandler(NativeMethods.zbar_image_data_handler_t handler, IntPtr userdata){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot set data handler. Unmanaged processor is null.");

			return NativeMethods.zbar_processor_set_data_handler(this._processor, handler, userdata);
		}

		/// <summary>
		/// Wait for input to the display window from the user.
		/// </summary>
		/// <param name="timeout">Timeout in ms, use -1 to wait forever.</param>
		/// <returns>True, when input is received</returns>
		public bool UserWait(int timeout){
			if(this._processor == IntPtr.Zero)
				throw new Exception("Cannot wait for user. Unmanaged processor is null.");

			int result = NativeMethods.zbar_processor_user_wait(this._processor, timeout);

			if(result == -1)
				throw new ZBarException(this._processor);

			return result != 0;
		}

		public bool Visible{
			get{
				if(this._processor == IntPtr.Zero)
					throw new Exception("Cannot get visible. Unmanaged processor is null.");

				int status = NativeMethods.zbar_processor_is_visible(this._processor);

				if(status == -1)
					throw new ZBarException(this._processor);

				return status != 0;
			}

			set{
				if(this._processor == IntPtr.Zero)
					throw new Exception("Cannot set visible. Unmanaged processor is null.");

				int result = NativeMethods.zbar_processor_set_visible(this._processor, value ? 1 : 0);
				if(result != 0)
					throw new ZBarException(this._processor);
			}
		}

		#region IDisposable implementation.

		public void Dispose(){
			// Copying pattern from Video.
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing){
			// Copying pattern from Video.
			if(this._processor != IntPtr.Zero){
				this.Destroy();
			}

			if(disposing){
				/// Nothing yet???
			}
		}

		~Processor(){
			this.Dispose();
		}

		#endregion

		#region Extern C functions.

		/// <summary>
		/// Native zbar_processor_* methods
		/// </summary>
		private static class NativeMethods
		{
			/// <summary>
			/// Image data handler delegate
			/// </summary>
			public delegate void zbar_image_data_handler_t(IntPtr image, IntPtr userdata);
			
			/// <summary>
			/// constructor.
			/// if threaded is set and threading is available the processor
			/// will spawn threads where appropriate to avoid blocking and
			/// improve responsiveness
			/// </summary>
			[DllImport("libzbar")]
			public static extern IntPtr zbar_processor_create(int threaded);

			/// <summary>
			/// destructor.  cleans up all resources associated with the processor
			/// </summary>
			[DllImport("libzbar")]
			public static extern void zbar_processor_destroy(IntPtr processor);
			
			/// <summary>
			/// retrieve decode results for last scanned image/frame.
			/// </summary>
			/// <returns> the symbol set result container or NULL if no results are available</returns>
			/// <remarks>
			/// the returned symbol set has its reference count incremented;
			/// ensure that the count is decremented after use
			/// </remarks>
			[DllImport("libzbar")]
			public static extern IntPtr zbar_processor_get_results(IntPtr processor);

			/// <summary>
			/// (re)initialization.
			/// opens a video input device and/or prepares to display output
			/// </summary>
			[DllImport("libzbar")]
			public static extern void zbar_processor_init(IntPtr processor, string video, int enable_display);

			/// <summary>
			/// retrieve the current state of the ouput window.
			/// </summary>
			/// <returns>
			/// 1 if the output window is currently displayed, 0 if not.
			/// -1 if an error occurs
			/// </returns>
			[DllImport("libzbar")]
			public static extern int zbar_processor_is_visible(IntPtr processor);

			/// <summary>
			/// parse configuration string using zbar_parse_config()
			/// and apply to processor using zbar_processor_set_config().
			/// </summary>
			/// <returns>
			/// 0 for success, non-0 for failure
			/// </returns>
			[DllImport("libzbar")]
			public static extern int zbar_processor_parse_config(IntPtr processor, string config_string);

			/// <summary>
			/// request a preferred size for the video image from the device.
			/// the request may be adjusted or completely ignored by the driver.
			/// </summary>
			/// <remarks>
			/// must be called before zbar_processor_init()
			/// </remarks>
			[DllImport("libzbar")]
			public static extern int zbar_processor_request_size(IntPtr processor, uint width, uint height);

			/// <summary>
			/// control the processor in free running video mode.
			/// only works if video input is initialized. if threading is in use,
			/// scanning will occur in the background, otherwise this is only
			/// useful wrapping calls to zbar_processor_user_wait(). if the
			/// library output window is visible, video display will be enabled
			/// </summary>
			[DllImport("libzbar")]
			public static extern int zbar_processor_set_active(IntPtr processor, int active);

			/// <summary>
			/// set config for indicated symbology (0 for all) to specified value.
			/// </summary>
			/// <returns>
			/// 0 for success, non-0 for failure (config does not apply to specified symbology, or value out of range)
			/// </returns>
			[DllImport("libzbar")]
			public static extern int zbar_processor_set_config(IntPtr processor, int symbology, int config, int value);

			/// <summary>
			/// setup result handler callback.
			/// the specified function will be called by the processor whenever
			/// new results are available from the video stream or a static image.
			/// pass a NULL value to disable callbacks.
			/// </summary>
			/// <param name="processor">
			/// the object on which to set the handler.
			/// </param>
			/// <param name="handler">
			/// the function to call when new results are available.
			/// </param>
			/// <param name="userdata">
			/// is set as with zbar_processor_set_userdata().
			/// </param>
			/// <returns>
			/// the previously registered handler
			/// </returns>
			[DllImport("libzbar")]
			public static extern IntPtr zbar_processor_set_data_handler(IntPtr processor, zbar_image_data_handler_t handler, IntPtr userdata);

			/// <summary>
			/// show or hide the display window owned by the library.
			/// the size will be adjusted to the input size
			/// </summary>
			[DllImport("libzbar")]
			public static extern int zbar_processor_set_visible(IntPtr processor, int visible);

			/// <summary>
			/// wait for input to the display window from the user (via mouse or keyboard).
			/// </summary>
			/// <returns>
			/// >0 when input is received, 0 if timeout ms expired
			/// with no input or -1 in case of an error
			/// </returns>
			[DllImport("libzbar")]
			public static extern int zbar_processor_user_wait(IntPtr processor, int timeout);
		}

		#endregion
	}
}
