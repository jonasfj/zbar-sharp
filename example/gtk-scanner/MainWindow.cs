using System;
using Gtk;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel){
		Build ();
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