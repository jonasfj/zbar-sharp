using System;

namespace ZBar
{
	public class ImageDataEventArgs : EventArgs
	{
		private Image _image;
		
		public ImageDataEventArgs(Image image, object userData)
		{
			if(this.Image == null)
				throw new ArgumentNullException("image");
			this.Image = image;
			this.UserData = userData;
		}

		public Image Image
		{
			get{
				return this._image;
			}
			set{
				if(value == null)
					throw new ArgumentNullException("value");
				this._image = value;
			}
		}

		public object UserData{get; set;}
	}
}
