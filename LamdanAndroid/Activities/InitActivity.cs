
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace goheja
{
	[Activity(Label = "InitActivity", ScreenOrientation = ScreenOrientation.Portrait)]
	public class InitActivity : BaseActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.InitActivity);

			FindViewById<TextView>(Resource.Id.ActionSignIn).Click += ActionSignIn;
			FindViewById<TextView>(Resource.Id.ActionSignUp).Click += ActionSignUp;
		}

		void ActionSignIn(object sender, EventArgs e)
		{
			var activity = new Intent(this, typeof(LoginActivity));
			StartActivityForResult(activity, 1);
		}

		void ActionSignUp(object sender, EventArgs e)
		{
			var activity = new Intent(this, typeof(RegisterActivity));
			StartActivityForResult(activity, 1);
		}
	}
}
