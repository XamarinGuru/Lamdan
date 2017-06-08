using Foundation;
using System;
using UIKit;
using PortableLibrary;
using SDWebImage;

namespace location2
{
	public partial class UserCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString("UserCell");
		public static readonly UINib Nib;

		static UserCell()
		{
			Nib = UINib.FromName("UserCell", NSBundle.MainBundle);
		}

		protected UserCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public void SetCell(Athlete user)
		{
			img1.Image = new UIImage();
			img2.Image = new UIImage();
			img3.Image = new UIImage();
			img4.Image = new UIImage();
			img5.Image = new UIImage();
			imgStatus.Image = new UIImage();
			imgPhoto.Image = UIImage.FromFile("icon_no_avatar.png");

			try
			{
				lblName.Text = user.name;
				if (!string.IsNullOrEmpty(user.userImagURI))
				{
					imgPhoto.SetImage(url: new NSUrl(user.userImagURI));
				}
			}
			catch
			{
			}

			foreach (var eventDoneToday in user.eventsDoneToday)
			{
                var imgTodayDone = new UIImageView();
				switch (eventDoneToday.eventType)
				{
					case "1":
						imgTodayDone.Image = UIImage.FromFile("icon_bike.png");
						break;
					case "2":
						imgTodayDone.Image = UIImage.FromFile("icon_run.png");
						break;
					case "3":
						imgTodayDone.Image = UIImage.FromFile("icon_swim.png");
						break;
					case "4":
						imgTodayDone.Image = UIImage.FromFile("icon_triathlon.png");
						break;
					case "5":
						imgTodayDone.Image = UIImage.FromFile("icon_other.png");
						break;
				}
			}
			switch (user.pmcStatus)
			{
				case 1:
					imgStatus.Image = UIImage.FromFile("icon_circle_green.png");
					break;
				case 2:
					imgStatus.Image = UIImage.FromFile("icon_circle_blue.png");
					break;
				case 3:
					imgStatus.Image = UIImage.FromFile("icon_circle_red.png");
					break;
				case 4:
					imgStatus.Image = UIImage.FromFile("icon_circle_empty.png");
					break;
			}
		}

		override public void LayoutSubviews()
		{
			base.LayoutSubviews();

			imgPhoto.LayoutIfNeeded();
			imgPhoto.Layer.CornerRadius = imgPhoto.Frame.Size.Width / 2;
			imgPhoto.Layer.MasksToBounds = true;
			imgPhoto.Layer.BorderWidth = 1;
			imgPhoto.Layer.BorderColor = UIColor.Gray.CGColor;
		}
	}
}