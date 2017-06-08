using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Android.Widget;
using PortableLibrary;
using UniversalImageLoader.Core;

namespace goheja
{
	public class UsersAdapter : BaseAdapter
	{
		List<Athlete> _athletes;
		List<Athlete> _searchAthletes;
		BaseActivity mSuperActivity;

		public UsersAdapter(List<Athlete> athletes, BaseActivity superActivity)
		{
			_athletes = athletes;
			_searchAthletes = athletes;
			mSuperActivity = superActivity;
		}

		public override int Count
		{
			get
			{
				return _searchAthletes.Count;
			}
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return null;
		}

		override public long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			try
			{
				var athletes = _searchAthletes[position];

				convertView = LayoutInflater.From(mSuperActivity).Inflate(Resource.Layout.item_User, null);

				convertView.FindViewById<TextView>(Resource.Id.lblName).Text = athletes.name;

				if (!string.IsNullOrEmpty(athletes.userImagURI))
				{
					var imgIcon = convertView.FindViewById<ImageView>(Resource.Id.imgProfile);
					ImageLoader imageLoader = ImageLoader.Instance;
					imageLoader.DisplayImage(athletes.userImagURI, imgIcon);
				}

				var ActionFakeUser = convertView.FindViewById<LinearLayout>(Resource.Id.ActionFakeUser);
				ActionFakeUser.Click += ActionChangeFakeUser;
				ActionFakeUser.Tag = athletes._id;

                var scrollView = convertView.FindViewById<HorizontalScrollView>(Resource.Id.todayDoneScrollView);
                var layout = new LinearLayout(mSuperActivity.ApplicationContext);

                foreach (var eventDoneToday in athletes.eventsDoneToday)
                {
                    var imgTodayDone = new ImageView(mSuperActivity.ApplicationContext);
                    switch (eventDoneToday.eventType)
                    {
                        case "1":
                            imgTodayDone.SetImageResource(Resource.Drawable.icon_bike);
                            break;
                        case "2":
                            imgTodayDone.SetImageResource(Resource.Drawable.icon_run);
                            break;
                        case "3":
                            imgTodayDone.SetImageResource(Resource.Drawable.icon_swim);
                            break;
                        case "4":
                            imgTodayDone.SetImageResource(Resource.Drawable.icon_triathlon);
                            break;
                        case "5":
                            imgTodayDone.SetImageResource(Resource.Drawable.icon_other);
                            break;
                    }

                    imgTodayDone.SetX(0);
                    int dimensionInDp = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 35, mSuperActivity.Resources.DisplayMetrics);
                    imgTodayDone.LayoutParameters = new ViewGroup.LayoutParams(dimensionInDp, ViewGroup.LayoutParams.MatchParent);
                    imgTodayDone.SetPadding(5, 5, 5, 5);
                    layout.AddView(imgTodayDone);

                    imgTodayDone.Click += ActionEventInstruction;
                    imgTodayDone.Tag = athletes._id + "," + eventDoneToday.eventId;
                }
                scrollView.AddView(layout);

				switch (athletes.pmcStatus)
				{
					case 1:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_green);
						break;
					case 2:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_blue);
						break;
					case 3:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_red);
						break;
					case 4:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_empty);
						break;
				}
			}
			catch (Exception ex)
			{
				//mSuperActivity.ShowTrackMessageBox(ex.Message);
			}

			return convertView;
		}

        private void ActionEventInstruction(object sender, EventArgs e)
        {
            var tags = (sender as ImageView).Tag.ToString().Split(new char[] { ',' });

            var fakeUserId = tags[0];
            var eventId = tags[1];

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    mSuperActivity.ShowLoadingView(Constants.MSG_LOADING_EVENT_DETAIL);

                    var eventDetail = mSuperActivity.GetEventDetail(eventId);

                    AppSettings.selectedEvent = eventDetail;
                    AppSettings.selectedEvent._id = eventId;

                    mSuperActivity.HideLoadingView();


                    var currentUser = AppSettings.CurrentUser;

                    if (currentUser.userId == fakeUserId)
                    {
                        currentUser.athleteId = null;
                        AppSettings.isFakeUser = false;
                        AppSettings.fakeUserName = string.Empty;
                    }
                    else
                    {
                        currentUser.athleteId = fakeUserId;
                        AppSettings.isFakeUser = true;
                        foreach (var tmpUser in _searchAthletes)
                        {
                            if (tmpUser._id == fakeUserId)
                                AppSettings.fakeUserName = tmpUser.name;
                        }
                    }

                    AppSettings.CurrentUser = currentUser;

                    var nextIntent = new Intent(mSuperActivity, typeof(EventInstructionActivity));
                    nextIntent.PutExtra("FromWhere", "CoachList");
                    mSuperActivity.StartActivityForResult(nextIntent, 0);

                });
        }

        public void PerformSearch(string strSearch)
		{
			strSearch = strSearch.ToLower();
			_searchAthletes = _athletes.Where(x => x.name.ToLower().Contains(strSearch)).ToList();
		}

		void ActionChangeFakeUser(object sender, EventArgs e)
		{
			var fakeUserId = (sender as LinearLayout).Tag.ToString();

			var currentUser = AppSettings.CurrentUser;

            if (currentUser.userId == fakeUserId)
            {
                currentUser.athleteId = null;
                AppSettings.isFakeUser = false;
                AppSettings.fakeUserName = string.Empty;
            }
            else
            {
                currentUser.athleteId = fakeUserId;
                AppSettings.isFakeUser = true;
				foreach (var tmpUser in _searchAthletes)
				{
					if (tmpUser._id == fakeUserId)
                        AppSettings.fakeUserName = tmpUser.name;
				}
            }

			AppSettings.CurrentUser = currentUser;

			var nextIntent = new Intent(mSuperActivity, typeof(SwipeTabActivity));
			mSuperActivity.StartActivityForResult(nextIntent, 0);
		}
	}

	public class UsersInSubGroupAdapter : BaseAdapter
	{
		List<AthleteInSubGroup> _athletes;
		List<AthleteInSubGroup> _searchAthletes;
		BaseActivity mSuperActivity;

		public UsersInSubGroupAdapter(List<AthleteInSubGroup> athletes, BaseActivity superActivity)
		{
			_athletes = athletes;
			_searchAthletes = athletes;
			mSuperActivity = superActivity;
		}

		public override int Count
		{
			get
			{
				return _searchAthletes.Count;
			}
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return null;
		}

		override public long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			try
			{
				var athletes = _searchAthletes[position];

				convertView = LayoutInflater.From(mSuperActivity).Inflate(Resource.Layout.item_User, null);

				convertView.FindViewById<TextView>(Resource.Id.lblName).Text = athletes.athleteName;

				if (!string.IsNullOrEmpty(athletes.userImagURI))
				{
					var imgIcon = convertView.FindViewById<ImageView>(Resource.Id.imgProfile);
					ImageLoader imageLoader = ImageLoader.Instance;
					imageLoader.DisplayImage(athletes.userImagURI, imgIcon);
				}

				var ActionFakeUser = convertView.FindViewById<LinearLayout>(Resource.Id.ActionFakeUser);
				ActionFakeUser.Click += ActionChangeFakeUser;
				ActionFakeUser.Tag = athletes.athleteId;

				var scrollView = convertView.FindViewById<HorizontalScrollView>(Resource.Id.todayDoneScrollView);
				var layout = new LinearLayout(mSuperActivity.ApplicationContext);

				foreach (var eventDoneToday in athletes.eventsDoneToday)
				{
					var imgTodayDone = new ImageView(mSuperActivity.ApplicationContext);
                    switch (eventDoneToday.eventType)
					{
						case "1":
							imgTodayDone.SetImageResource(Resource.Drawable.icon_bike);
							break;
						case "2":
							imgTodayDone.SetImageResource(Resource.Drawable.icon_run);
							break;
						case "3":
							imgTodayDone.SetImageResource(Resource.Drawable.icon_swim);
							break;
						case "4":
							imgTodayDone.SetImageResource(Resource.Drawable.icon_triathlon);
							break;
						case "5":
							imgTodayDone.SetImageResource(Resource.Drawable.icon_other);
							break;
					}

					imgTodayDone.SetX(0);
                    imgTodayDone.LayoutParameters = new ViewGroup.LayoutParams(50, ViewGroup.LayoutParams.MatchParent);
                    imgTodayDone.LayoutParameters.Width = imgTodayDone.LayoutParameters.Height;
					imgTodayDone.SetPadding(5, 5, 5, 5);
					layout.AddView(imgTodayDone);


					imgTodayDone.Click += ActionEventInstruction;
                    imgTodayDone.Tag = athletes.athleteId + "," + eventDoneToday.eventId;
				}
				scrollView.AddView(layout);

				switch (athletes.pmcStatus)
				{
					case 1:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_green);
						break;
					case 2:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_blue);
						break;
					case 3:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_red);
						break;
					case 4:
						convertView.FindViewById<ImageView>(Resource.Id.imgStatus).SetImageResource(Resource.Drawable.icon_circle_empty);
						break;
				}
			}
			catch (Exception ex)
			{
				//mSuperActivity.ShowTrackMessageBox(ex.Message);
			}

			return convertView;
		}

        private void ActionEventInstruction(object sender, EventArgs e)
        {
            var tags = (sender as ImageView).Tag.ToString().Split(new char[] { ',' });

            var fakeUserId = tags[0];
            var eventId = tags[1];

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    mSuperActivity.ShowLoadingView(Constants.MSG_LOADING_EVENT_DETAIL);

                    var eventDetail = mSuperActivity.GetEventDetail(eventId);

                    AppSettings.selectedEvent = eventDetail;
                    AppSettings.selectedEvent._id = eventId;

                    mSuperActivity.HideLoadingView();


                    var currentUser = AppSettings.CurrentUser;

                    if (currentUser.userId == fakeUserId)
                    {
                        currentUser.athleteId = null;
                        AppSettings.isFakeUser = false;
                        AppSettings.fakeUserName = string.Empty;
                    }
                    else
                    {
                        currentUser.athleteId = fakeUserId;
                        AppSettings.isFakeUser = true;
                        foreach (var tmpUser in _searchAthletes)
                        {
                            if (tmpUser.athleteId == fakeUserId)
                                AppSettings.fakeUserName = tmpUser.athleteName;
                        }
                    }

                    AppSettings.CurrentUser = currentUser;

                    var nextIntent = new Intent(mSuperActivity, typeof(EventInstructionActivity));
                    nextIntent.PutExtra("FromWhere", "CoachList");
                    mSuperActivity.StartActivityForResult(nextIntent, 0);

                });
        }

		public void PerformSearch(string strSearch)
		{
			strSearch = strSearch.ToLower();
			_searchAthletes = _athletes.Where(x => x.athleteName.ToLower().Contains(strSearch)).ToList();
		}

		void ActionChangeFakeUser(object sender, EventArgs e)
		{
			var fakeUserId = (sender as LinearLayout).Tag.ToString();

			var currentUser = AppSettings.CurrentUser;

			if (currentUser.userId == fakeUserId)
			{
				currentUser.athleteId = null;
				AppSettings.isFakeUser = false;
                AppSettings.fakeUserName = string.Empty;
			}
			else
			{
				currentUser.athleteId = (sender as LinearLayout).Tag.ToString();
				AppSettings.isFakeUser = true;
				foreach (var tmpUser in _searchAthletes)
				{
                    if (tmpUser.athleteId == fakeUserId)
                        AppSettings.fakeUserName = tmpUser.athleteName;
				}
			}

			AppSettings.CurrentUser = currentUser;

			var nextIntent = new Intent(mSuperActivity, typeof(SwipeTabActivity));
			mSuperActivity.StartActivityForResult(nextIntent, 0);
		}
	}
}
