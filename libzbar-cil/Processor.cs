using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZBar
{
	public sealed class Processor:
		IDisposable
	{
		private const int verbosity = 10;

		private IntPtr processor_;

		public Processor(bool threaded)
		{
			this.processor_ = zbar_processor_create(threaded ? 1 : 0);

			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Couldn't create unmanaged processor." +
						"Reason unknown.");
			}
		}

		public Processor(bool threaded, uint width, uint height):
			this(threaded)
		{
			this.RequestSize(width, height);
		}

		private void Destroy()
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot destroy. Unmanaged processor is null.");
			}

			zbar_processor_set_active(this.processor_, 0);
			zbar_processor_destroy(this.processor_);

			this.processor_ = IntPtr.Zero;
		}
		
		public event EventHandler<ImageDataEventArgs> ImageData;

		/// <remarks>
		/// No clue how to factor the user data into things safely mixing
		/// managed types with pointers. For now, we'll just skip that.
		/// </remarks>
		private void ImageDataHandler(
				IntPtr imagePtr)
		{
			if(imagePtr == IntPtr.Zero)
				throw new ArgumentNullException("imagePtr");
			
			using(Image image = Image.Wrap(imagePtr))
			{				
				this.OnImageData(
						new ImageDataEventArgs(image, null));
			}
		}

		public void Init(string video, bool enableDisplay)
		{
			zbar_processor_init(
					this.processor_,
					video,
					enableDisplay ? 1 : 0);
		}
		
		private void OnImageData(ImageDataEventArgs e)
		{
			if(this.ImageData != null)
				this.ImageData(this, e);
		}

		public void ParseConfig(string configString)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot parse config. Unmanaged processor " +
						"is null.");
			}

			var result = zbar_processor_parse_config(
					this.processor_,
					configString);

			if(result != 0)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Cannot parse unmanaged processor config. " +
						"Reason unknown.");
			}
		}

		public void RequestSize(uint width, uint height)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot request size. Unmanaged processor " +
						"is null.");
			}

			var result = zbar_processor_request_size(
					this.processor_,
					width,
					height);

			if(result != 0)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Cannot request unmanaged processor size. " +
						"Reason unknown.");
			}
		}

		public void SetActive(bool active)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot set active. Unmanaged processor " +
						"is null.");
			}

			var result = zbar_processor_set_active(
					this.processor_,
					active ? 1 : 0);

			if(result != 0)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Cannot set unmanaged processor active." +
						"Reason unknown.");
			}
		}

		public void SetConfig(
				SymbolType symbology,
				Config config,
				int value)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot set config. Unmanaged processor " +
						"is null.");
			}

			var result = zbar_processor_set_config(
					this.processor_,
					(int)symbology,
					(int)config,
					value);

			if(result != 0)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Cannot set unmanaged processor config. " +
						"Reason unknown.");
			}
		}

		private IntPtr SetDataHandler(
				zbar_image_data_handler_t handler,
				IntPtr userdata)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot set data handler. Unmanaged processor " +
						"is null.");
			}

			return zbar_processor_set_data_handler(
					this.processor_,
					handler,
					userdata);
		}

		/// <summary>
		/// Wait for input to the display window from the user.
		/// </summary>
		/// <param name="timeout">Use -1 to wait forever. Unknown units
		/// (likely s or ms).</param>
		/// <returns>Zero when input is received. Non-zero otherwise.</returns>
		public int UserWait(int timeout)
		{
			if(this.processor_ == IntPtr.Zero)
			{
				throw new Exception(
						"Cannot wait for user. Unmanaged processor " +
						"is null.");
			}

			var result = zbar_processor_user_wait(
					this.processor_,
					timeout);

			if(result == -1)
			{
				throw new Exception(zbar_processor_error_string(
						this.processor_,
						verbosity) ??
						"Failed to wait for user. Reason unknown.");
			}

			return result;
		}

		public bool Visible
		{
			get
			{
				if(this.processor_ == IntPtr.Zero)
				{
					throw new Exception(
							"Cannot get visible. Unmanaged processor " +
							"is null.");
				}

				var status = zbar_processor_is_visible(this.processor_);

				if(status == -1)
				{
					throw new Exception(zbar_processor_error_string(
							this.processor_,
							verbosity) ??
							"Failed to get visible. Reason unknown.");
				}

				return status != 0;
			}

			set
			{
				if(this.processor_ == IntPtr.Zero)
				{
					throw new Exception(
							"Cannot set visible. Unmanaged processor " +
							"is null.");
				}

				var result = zbar_processor_set_visible(
						this.processor_,
						value ? 1 : 0);

				if(result != 0)
				{
					throw new Exception(zbar_processor_error_string(
							this.processor_,
							verbosity) ??
							"Cannot set unmanaged processor visible. " +
							"Reason unknown.");
				}
			}
		}

		#region IDisposable implementation.

		public void Dispose()
		{
			// Copying pattern from Video.
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			// Copying pattern from Video.
			if(this.processor_ != IntPtr.Zero)
			{
				this.Destroy();
			}

			if(disposing)
			{
				/// Nothing yet???
			}
		}

		~Processor()
		{
			this.Dispose();
		}

		#endregion

		#region Extern C functions.

		public delegate void zbar_image_data_handler_t(
				IntPtr image,
				IntPtr userdata);

		[DllImport("libzbar")]
		private static extern IntPtr zbar_processor_create(int threaded);

		[DllImport("libzbar")]
		private static extern void zbar_processor_destroy(
				IntPtr processor);

		//[DllImport("libzbar")]
		//private static extern string zbar_processor_error_string(
		//		IntPtr processor,
		//		int verbosity);
		private static string zbar_processor_error_string(
				IntPtr processor,
				int verbosity)
		{
			return _zbar_error_string(processor, verbosity);
		}

		[DllImport("libzbar")]
		private static extern string _zbar_error_string(
				IntPtr object_,
				int verbosity);

		[DllImport("libzbar")]
		private static extern IntPtr zbar_processor_get_results(
				IntPtr processor);

		[DllImport("libzbar")]
		private static extern void zbar_processor_init(
				IntPtr processor,
				string video,
				int enable_display);

		[DllImport("libzbar")]
		private static extern int zbar_processor_is_visible(IntPtr processor);

		[DllImport("libzbar")]
		private static extern int zbar_processor_parse_config(
				IntPtr processor,
				string config_string);

		[DllImport("libzbar")]
		private static extern int zbar_processor_request_size(
				IntPtr processor,
				uint width,
				uint height);

		[DllImport("libzbar")]
		private static extern int zbar_processor_set_active(
				IntPtr processor,
				int active);

		[DllImport("libzbar")]
		private static extern int zbar_processor_set_config(
				IntPtr processor,
				int symbology,
				int config,
				int value);

		[DllImport("libzbar")]
		private static extern IntPtr zbar_processor_set_data_handler(
				IntPtr processor,
				zbar_image_data_handler_t handler,
				IntPtr userdata);

		[DllImport("libzbar")]
		private static extern int zbar_processor_set_visible(
				IntPtr processor,
				int visible);

		[DllImport("libzbar")]
		private static extern int zbar_processor_user_wait(
				IntPtr processor,
				int timeout);

		#endregion
	}
}
