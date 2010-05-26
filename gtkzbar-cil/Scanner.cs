using ZBar;
using System;
using Gtk;
using Gdk;
using System.Threading;
using System.Media;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

/// <summary>
/// GtkZBar contains the Scanner widget for scanning bar codes using ZBar in Gtk#
/// </summary>
namespace GtkZBar
{
	/// <summary>
	/// Bar code scanner widget
	/// </summary>
	/// <remarks>
	/// This widget uses a thread to pull the video device, so it is important to destroy it correctly.
	/// </remarks>
	[System.ComponentModel.ToolboxItem(true)]
	public class Scanner : Gtk.DrawingArea
	{	
		public Scanner(){
			this.Destroyed += HandleDestroyed; 
		}

		void HandleDestroyed(object sender, EventArgs e){
			//Make sure we're closed
			if(this.worker != null){
				//Don't call close, it'll force a redraw
				this.worker.Abort();
				this.worker.Join();
			}
			this.worker = null;
			
			//Release static image resources
			if(this.overlay != null)
				this.overlay.Dispose();
			this.overlay = null;
			if(this.sourceMissing != null)
				this.sourceMissing.Dispose();
			this.sourceMissing = null;
		}
		
		/// <summary>
		/// Temporary variable used to transfer argument to the ProcessVideo method
		/// </summary>
		private string currentDevice = null;
		
		/// <summary>
		/// Open a video device
		/// </summary>
		/// <param name="device">
		/// Video device to open
		/// </param>
		public void Open(string device){
			if(this.worker != null)
				this.worker.Abort();
			this.worker = new Thread(this.ProcessVideo);
			this.currentDevice = device;
			this.worker.Start();
		}
		
		public void Close(){
			if(this.worker != null){
				this.worker.Abort();
				this.worker.Join();
				lock(this.drawLock){
					this.toDraw = null;
					this.symbols = null;
				}
				this.QueueDraw();
			}
			this.worker = null;
			this.currentDevice = null;
		}

		/// <summary>
		/// Reference to the worker thread, so that we can kill it :)
		/// </summary>
		private System.Threading.Thread worker = null;
		
		/// <summary>
		/// Image access lock
		/// </summary>
		private System.Object drawLock = new System.Object();
		
		/// <summary>
		/// Image to draw
		/// </summary>
		/// <remarks>Lock this using drawLock</remarks>
		private byte[] toDraw = null;
		
		/// <summary>
		/// Image width
		/// </summary>
		/// <remarks>Lock this using drawLock</remarks>
		private int toDrawWidth;
		
		/// <summary>
		/// Image height
		/// </summary>
		/// <remarks>Lock this using drawLock</remarks>
		private int toDrawHeight;
		
		/// <summary>
		/// Symbols found in the image
		/// </summary>
		/// <remarks>Lock this using drawLock</remarks>
		private List<Symbol> symbols = null;
		
		/// <summary>
		/// Process video, the worker thread
		/// </summary>
		private void ProcessVideo(){
			using(Video video = new Video()){
				try{
					video.Open(this.currentDevice);
					video.Enabled = true;
					using(ImageScanner scanner = new ImageScanner()){
						scanner.Cache = true;
						this.CaptureVideo(video, scanner);
					}
					video.Enabled = false;
				}
				catch(ZBarException ex){
					lock(this.drawLock){
						this.toDraw = null;
						this.symbols = null;
					}
					GLib.IdleHandler hdl = delegate(){
						if(this.Stopped != null)
							this.Stopped(this, new EventArgs());
						if(this.Error != null)
							this.Error(this, new ErrorEventArgs(ex.Message, ex));
						this.QueueDraw();
						return false;
					};
					GLib.Idle.Add(hdl);
				}
			}
		}
		
		/// <summary>
		/// Capture and scan images
		/// </summary>
		/// <remarks>
		/// This method will also flip the images so that they need not be flipped when drawing.
		/// </remarks>
		private void CaptureVideo(Video video, ImageScanner scanner){
			while(true){
				using(ZBar.Image frame = video.NextFrame()){
					using(ZBar.Image bwImg = frame.Convert(0x30303859)){
						//Scan the image for bar codes
						scanner.Scan(bwImg);
						//Get the data, width, height and symboles for the image
						byte[] data = bwImg.Data;
						int w = (int)bwImg.Width;
						int h = (int)bwImg.Height;
						var symbols = new List<Symbol>(bwImg.Symbols);
						//Flip the image vertically, if needed
						if(this.Flip){
							for(int ih = 0; ih < h; ih++){
								for(int iw = 0; iw < w / 2; iw++){
									//TODO: The offsets below could be computed more efficiently, but I don't care this happens on a thread... :)
									int p1 = w * ih + iw;
									int p2 = w * ih + (w - iw- 1);
									//Swap bytes:
									byte b1 = data[p1];
									data[p1] = data[p2];
									data[p2] = b1;
								}
							}
						}
						//Lock the drawing process and pass it the data we aquired
						lock(this.drawLock){
							this.toDraw = data;
							this.toDrawWidth = w;
							this.toDrawHeight = h;
							this.symbols = symbols;
						}
						this.ThreadSafeRedraw();
					}
				}
			}
		}
				
		/// <summary>
		/// Simple threadsafe redraw method
		/// </summary>
		private void ThreadSafeRedraw(){
			GLib.IdleHandler hdl = delegate(){
				this.QueueDraw();
				return false;
			};
			GLib.Idle.Add(hdl);
		}
		
		/// <summary>
		/// Occurs if an error occured during runtime and the video device stopped
		/// </summary>
		public event EventHandler Stopped;
		
		/// <summary>
		/// Occurs if an error happens and user should be informed about this
		/// </summary>
		public event EventHandler<ErrorEventArgs> Error;
		
		/// <summary>
		/// Occurs whenever a bar code have been successfully scanned
		/// </summary>
		/// <remarks>Usually hardware related issues, as this is stuff the user must handle.</remarks>
		public event EventHandler<BarScannedArgs> BarScanned;

		private string data = null;
		private const int overlayFrameCount = 35;
		private int overlayingFrames = 0;
		private Pixbuf overlay = Pixbuf.LoadFromResource("gtkzbarcil.check.png");
		private Pixbuf sourceMissing = Pixbuf.LoadFromResource("gtkzbarcil.webcam.png");
		
		/// <summary>
		/// Resets the last item scanned
		/// </summary>
		/// <remarks>This method may only be called from UI-thread.</remarks>
		public void ResetLastItemScanned(){
			this.data = "";
		}
		
		protected override bool OnExposeEvent(Gdk.EventExpose ev)
		{
			Gdk.Window win = ev.Window;
			Gdk.Rectangle rect = ev.Area;
			Gdk.GC gc = this.Style.BaseGC(StateType.Normal);
			lock(this.drawLock){
				if(this.toDraw != null){
					//Raise events for the symbols...
					bool gotSymbol = false;
					//See if there's a new symbol
					if(this.symbols != null){
						foreach(Symbol s in this.symbols){
							if(s.Count > 0 && this.data != s.ToString()){
								this.data = s.ToString();
								//Don't raise it inside the expose event :)
								GLib.IdleHandler raiser = delegate(){
									if(this.BarScanned != null)
										this.BarScanned(this, new BarScannedArgs(s));
									return false;
								};
								GLib.Idle.Add(raiser);
								gotSymbol = true;
							}
						}
					}
					//Avoid beeping more than once..
					if(gotSymbol){
						if(!this.Mute)
							System.Media.SystemSounds.Beep.Play();
						if(this.overlayingFrames == 0){
							GLib.TimeoutHandler hdl = delegate(){
								this.QueueDraw();
								this.overlayingFrames -= 1;
								return this.overlayingFrames > 0;
							};
							GLib.Timeout.Add(35, hdl);
						}
						//Start drawing an overlay
						this.overlayingFrames = overlayFrameCount;
					}
					this.symbols = null; //Symbols have been handled
					
					//See if we want to request a resize
					if(this.reqHeight != this.toDrawHeight ||
					   this.reqWidth != this.toDrawWidth){
						this.reqHeight = this.toDrawHeight;
						this.reqWidth = this.toDrawWidth;
						this.QueueResize();
					}
					
					//Draw the gray image
					int w = Math.Min(rect.Width, this.toDrawWidth);
					int h = Math.Min(rect.Height, this.toDrawHeight);
					
					//Draw the image
					win.DrawGrayImage(gc, 0, 0, w, h, Gdk.RgbDither.Normal, this.toDraw, this.toDrawWidth);
					
					if(this.overlayingFrames > 0){
						w = Math.Min(this.AllocatedWidth, (int)this.overlay.Width);
						h = Math.Min(this.AllocatedHeight, (int)this.overlay.Height);
						using(Gdk.Pixbuf pix = new Pixbuf(Colorspace.Rgb, true, 8, w, h)){
							pix.Fill(0x00000000); //Fill with invisibility :)
							this.overlay.Composite(pix, 0, 0, w, h, 0, 0, 1, 1, InterpType.Bilinear, 255 / 35 * this.overlayingFrames);
							win.DrawPixbuf(gc, pix, 0, 0,
							               (this.AllocatedWidth - w) / 2, 
							               (this.AllocatedHeight - h) / 2, w, h, RgbDither.Normal, 0, 0);
						}
					}
				}else{
					win.DrawRectangle(gc, true, rect);
					
					int w = Math.Min(this.AllocatedWidth, (int)this.sourceMissing.Width);
					int h = Math.Min(this.AllocatedHeight, (int)this.sourceMissing.Height);
					
					Rectangle img = new Rectangle((this.AllocatedWidth - w) / 2,
					                              (this.AllocatedHeight - h) / 2,
					                              w, h);
					Rectangle target = Rectangle.Intersect(img, rect);
					if(target != Rectangle.Zero){
						win.DrawPixbuf(gc, this.sourceMissing, 
						               Math.Max(target.X - img.X, 0), 
						               Math.Max(target.Y - img.Y, 0), 
						               target.X,
						               target.Y,
						               target.Width,
						               target.Height,
						               RgbDither.Normal, 0, 0);
					}
				}
			}
			return true;
		}
		
		protected override void OnSizeAllocated(Gdk.Rectangle allocation){
			base.OnSizeAllocated(allocation);
			this.AllocatedWidth = allocation.Width;
			this.AllocatedHeight = allocation.Height;
		}
		
		private int AllocatedWidth;
		private int AllocatedHeight;
		
		private int reqHeight = 200;
		private int reqWidth = 200;
		
		protected override void OnSizeRequested(ref Gtk.Requisition requisition){
			// Calculate desired size here.
			requisition.Height = this.reqHeight;
			requisition.Width = this.reqWidth;
		}
		
		/// <value>
		/// Mute
		/// </value>
		public bool Mute{get; set;}
		
		/// <value>
		/// Flip the image vertically, default false
		/// </value>
		/// <remarks>
		/// Our studies have shown that it is a lot easier to scan bar codes when the webcam is facing you
		/// if the image is flipped.
		/// </remarks>
		public bool Flip{get; set;}
		
		/// <summary>
		/// List potential video sources on the system
		/// </summary>
		/// <remarks>
		/// This just adds a default device without checking it's existance on Windows.
		/// On Linux this list all /dev/video* devices, and attempts to get their name
		/// using /lib/udev/v4l_id, watch the kernel source (link available in source comments)
		/// to that this remains compatible.
		/// 
		/// Alternately, define UDEV_RULES and the "/dev/v4l/by-id/" symlinks will be used
		/// to find the names. Note this presumably depends on Ubuntu 9.10 udev rules.
		/// Thus the other approach is preffered.
		/// 
		/// Note that on Linux this might be solve a lot better using HAL over dbus,
		/// however, last I checked HAL is being replaced by DeviceKit which isn't done
		/// yet, so unless the APIs are expected to remain stable HAL is a waste of time.
		/// </remarks>
		/// <returns>
		/// A <see cref="IDictionary"/> of Name and device
		/// </returns>
		public static IDictionary<string, string> ListVideoSources(){
			var retval = new Dictionary<string, string>();
			//Runtime platform check, see http://www.mono-project.com/FAQ:_Technical
			int p = (int) Environment.OSVersion.Platform;
            if((p == (int)PlatformID.Unix) || (p == (int)PlatformID.MacOSX) || (p == 128)) {
				//Assume we're on Linux, if it's MacOS X we'll die because libzbar is missing
				try{
					//This can probably be done smarter, using HAL, however, AFAIK
					//it is in the process of being replaced by DeviceKit so let's
					//not start touching something that is unlikely to work...
#if UDEV_RULES
					try{
						//A small hack that depends on Ubuntu's udev rules, to find a name
						//See /lib/udev/rules.d/60-persistent-v4l.rules
						foreach(var device in Directory.GetFiles("/dev/v4l/by-id/")){
							var name = Path.GetFileName(device);
							name = name.Remove(0, name.IndexOf("_") + 1);
							name = name.Remove(name.IndexOf("-video-index"));
							if(name == string.Empty)
								throw new Exception("Hack that jumps aways from here :) ");
							retval.Add(name, device);
						}
					}
					catch{
						//rescue if the hack fails... :)
						foreach(var device in Directory.GetFiles("/dev/", "video*")){
							//I can at least add a space to make it look good :)
							var name = "Video " + Path.GetFileName(device).Remove(0, 5);
							retval.Add(name, device);
						}
					}
#else
					foreach(var device in Directory.GetFiles("/dev/", "video*")){
						string name = null;
						//Assume it supports capture
						bool supports_capture = true;
						
						//Attempt to use /lib/udev/v4l_id a small program used in udev rules, but seems to be available to userspace
						//v4l_id is afaik not documented, but it's short and the source can be found in extras/v4l_id/v4l_id.c
						//of the udev development tree: linux/hotplug/udev.git
						//See: http://git.kernel.org/?p=linux/hotplug/udev.git;a=blob;f=extras/v4l_id/v4l_id.c;hb=HEAD
						try{
							using(Process v4l_id = new Process()){
								v4l_id.StartInfo.UseShellExecute = false;
								v4l_id.StartInfo.FileName = "/lib/udev/v4l_id";
								v4l_id.StartInfo.CreateNoWindow = true;
								v4l_id.StartInfo.Arguments = device;
								v4l_id.StartInfo.RedirectStandardOutput = true;
								v4l_id.Start();
								string input;
								while((input = v4l_id.StandardOutput.ReadLine()) != null){
									if(input.StartsWith("ID_V4L_PRODUCT="))
										name = input.Remove(0, 15);
									if(input.StartsWith("ID_V4L_CAPABILITIES="))
										supports_capture = input.Contains(":capture:");
								}
								v4l_id.WaitForExit();
								if(v4l_id.ExitCode != 0)
									throw new Exception("Don't use anything we've got here...");
							}
						}
						catch(Exception e){
							//I can at least add a space to make it look good :)
							name = "Video " + System.IO.Path.GetFileName(device).Remove(0, 5);
						}
						
						if(supports_capture)
							retval.Add(name, device);
					}
#endif
				}
				catch{
					//Just ignore any exceptions and add the default video device,
					//if case we're on a distro/os that doesn't use /dev/video*
					//for video devices... udev rules maybe... The world is twisted place...
					retval.Add("Default video device", "/dev/video0");
				}
			}else{
				//Windows
				retval.Add("Default video device", "/dev/video0");
				//Don't care how to find video devices on Windows(R),
				//but /dev/video0 opens the default device... It is a undocumented
				//feature of libzbar, see: zbar-0.10/zbar/video/v4w.c, line 442 - 445
			}
			return retval;
		}
	}
	
	/// <summary>
	/// Event arguments for the Error event
	/// </summary>
	public class ErrorEventArgs : System.EventArgs{
		/// <value>
		/// Message that the user should be presented with
		/// </value>
		public string Message{get; private set;}
		
		/// <value>
		/// Inner exception
		/// </value>
		public ZBarException Exception{get; private set;}
		
		public ErrorEventArgs(string message){
			this.Message = message;
		}
		
		public ErrorEventArgs(string message, ZBarException innerException) : this(message) {
			this.Exception = innerException;
		}
	}
}
