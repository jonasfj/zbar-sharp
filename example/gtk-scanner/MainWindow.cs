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
using Gtk;

namespace Example.GtkScanner{

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel){
		Build();
		
		this.Scanner.Error += new EventHandler<GtkZBar.ErrorEventArgs>(this.OnScannerError);
		this.Scanner.BarScanned += new EventHandler<GtkZBar.BarScannedArgs>(this.OnScannerBarScanned);
			
		this.Scanner.Open("/dev/video0");
		Scanner.Rotate = true;
		this.VideoDevEntry.Text = "/dev/video0";
		this.UpdateMuteButton();
	}
	
	protected void OnDeleteEvent(object sender, DeleteEventArgs a){
		this.Scanner.Close();
		this.muteImage.Destroy();
		this.audioImage.Destroy();
		Application.Quit();
		a.RetVal = true;
	}

	protected virtual void OnScannerBarScanned (object sender, GtkZBar.BarScannedArgs e){
		this.ResultView.Buffer.Text = "Scanned: " + e.Symbol.ToString() + "\n" + this.ResultView.Buffer.Text;
	}

	protected virtual void OnOpenButtonClicked (object sender, System.EventArgs e){
		this.Scanner.Open(this.VideoDevEntry.Text);
	}

	protected virtual void OnScannerError (object sender, GtkZBar.ErrorEventArgs e){
		Console.WriteLine("Scanner error: " + e.Message);
	}
	
	//Load images from resources
	private Gtk.Image muteImage = new Gtk.Image(null, "muted.png");
	private Gtk.Image audioImage = new Gtk.Image(null, "audio.png");
	
	protected virtual void OnMuteButtonClicked (object sender, System.EventArgs e){
		this.Scanner.Mute = !this.Scanner.Mute;
		this.UpdateMuteButton();
	}
	
	private void UpdateMuteButton(){
		if(this.Scanner.Mute)
			this.MuteButton.Image = this.muteImage;
		else
			this.MuteButton.Image = this.audioImage;
	}

	protected virtual void OnFlipButtonClicked (object sender, System.EventArgs e){
		this.Scanner.Flip = this.FlipButton.Active;
	}
	
	protected virtual void OnRotateButtonClicked (object sender, System.EventArgs e){
		this.Scanner.Rotate = this.RotateButton.Active;
	}
}
	
}