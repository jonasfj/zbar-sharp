using System;

namespace ZBar
{
	public class ImageDataEventArgs:
		EventArgs
	{
		private Image image_;
		private object userData_;

		public ImageDataEventArgs(Image image, object userData)
		{
			if(this.Image == null)
				throw new ArgumentNullException("image");
			
			this.Image = image;
			this.UserData = userData;
		}

		public Image Image
		{
			get
			{
				return this.image_;
			}

			set
			{
				if(value == null)
					throw new ArgumentNullException("value");
				
				this.image_ = value;
			}
		}

		public object UserData
		{
			get
			{
				return this.userData_;
			}

			set
			{
				this.userData_ = value;
			}
		}
	}
}
