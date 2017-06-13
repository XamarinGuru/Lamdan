﻿using Foundation;
using UIKit;
using System;
using System.Threading;
using System.Threading.Tasks;
using EventKit;
using PortableLibrary;
using System.Collections.Generic;
using Google.Maps;

using Firebase.InstanceID;
using Firebase.Analytics;
using Firebase.CloudMessaging;

using UserNotifications;

namespace location2
{
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
	{
        //public event EventHandler<UserInfoEventArgs> NotificationReceived;

		public static LocationHelper MyLocationHelper = new LocationHelper();

		private nint bgThread = -1;

		public BaseViewController baseVC;

		EKCalendar goHejaCalendar = null;

		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

			MapServices.ProvideAPIKey(PortableLibrary.Constants.GOOGLE_MAP_API_KEY);

			// Monitor token generation
			InstanceId.Notifications.ObserveTokenRefresh(TokenRefreshNotification);

			// Register your app for remote notifications.
			if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
			{
				// iOS 10 or later
				var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
				UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
				{
					Console.WriteLine(granted);
				});

				// For iOS 10 display notification (sent via APNS)
				UNUserNotificationCenter.Current.Delegate = this;

				// For iOS 10 data message (sent via FCM)
				Messaging.SharedInstance.RemoteMessageDelegate = this;
			}
			else
			{
				// iOS 9 or before
				var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
				var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			}

			UIApplication.SharedApplication.RegisterForRemoteNotifications();

            App.Configure();

            ConnectToFCM();

			return true;
		}
		public override void WillEnterForeground(UIApplication application)
		{
			//ConnectToFCM (Window.RootViewController);
		}

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            base.ReceivedLocalNotification(application, notification);
        }



		// To receive notifications in foregroung on iOS 9 and below.
		// To receive notifications in background in any iOS version
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			Console.WriteLine("WillPresentNotification===" + userInfo);
			// If you are receiving a notification message while your app is in the background,
			// this callback will not be fired 'till the user taps on the notification launching the application.

			// If you disable method swizzling, you'll need to call this method. 
			// This lets FCM track message delivery and analytics, which is performed
			// automatically with method swizzling enabled.
			//Messaging.GetInstance ().AppDidReceiveMessage (userInfo);

			//if (NotificationReceived == null)
			//  return;

			//var e = new UserInfoEventArgs { UserInfo = userInfo };
			//NotificationReceived(this, e);
		}


		// You'll need this method if you set "FirebaseAppDelegateProxyEnabled": NO in GoogleService-Info.plist
		//public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
		//{
		//  InstanceId.SharedInstance.SetApnsToken (deviceToken, ApnsTokenType.Sandbox);
		//}

		// To receive notifications in foreground on iOS 10 devices.
		[Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
		public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			Console.WriteLine("WillPresentNotification===" + notification.Request.Content.UserInfo);
			//if (NotificationReceived == null)
			//  return;

			//var e = new UserInfoEventArgs { UserInfo = notification.Request.Content.UserInfo };
			//NotificationReceived(this, e);
		}
		#region Workaround for handling notifications in background for iOS 10
		[Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
		public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			Console.WriteLine("DidReceiveNotificationResponse===" + response.Notification.Request.Content.UserInfo);
			//var notification = new UILocalNotification();
			//if (NotificationReceived == null)
			//  return;

			//var e = new UserInfoEventArgs { UserInfo = response.Notification.Request.Content.UserInfo };
			//NotificationReceived(this, e);
		}
		#endregion
		// Receive data message on iOS 10 devices.
		public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage)
		{
			Console.WriteLine("ApplicationReceivedRemoteMessage===" + remoteMessage.AppData);
			var notification = new UILocalNotification();
			notification.AlertAction = "go to Lamdan";
			notification.AlertTitle = "title";
			notification.AlertBody = "body";
			notification.FireDate = NSDate.FromTimeIntervalSinceNow(5);
			notification.AlertLaunchImage = "icon_notification.png";
			notification.SoundName = UILocalNotification.DefaultSoundName;
			UIApplication.SharedApplication.ScheduleLocalNotification(notification);
		}

		//////////////////
		////////////////// WORKAROUND
		//////////////////

		

		

		

		void TokenRefreshNotification(object sender, NSNotificationEventArgs e)
		{
			var refreshedToken = InstanceId.SharedInstance.Token;

			ConnectToFCM();

			// TODO: If necessary send token to application server.
		}

		public static void ConnectToFCM()
		{
			Messaging.SharedInstance.Connect(error =>
			{
				if (error != null)
				{
					Console.WriteLine($"Token: {InstanceId.SharedInstance.Token}");
				}
				else
				{
					Console.WriteLine($"Token: {InstanceId.SharedInstance.Token}");
				}
			});
		}

		












        public override void DidEnterBackground(UIApplication application)
        {
            if (AppSettings.CurrentUser == null || baseVC == null)
                return;

            //Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.

            //Messaging.SharedInstance.Disconnect();
            //Console.WriteLine("Disconnected from FCM");

            DeviceCalendar.Current.EventStore.RequestAccess(EKEntityType.Event,
                (bool granted, NSError e) =>
                {
                    InvokeOnMainThread(() =>
                    {
                        if (granted)
                            UpdateCalendarTimer();
                    });
                });
        }

		private void UpdateCalendarTimer()
		{
			if (bgThread == -1)
			{
				bgThread = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
				new Task(() => { new Timer(UpdateCalendar, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(60 * 30)); }).Start();
			}
		}
		private void UpdateCalendar(object state)
		{
			InvokeOnMainThread(() => { AddGoHejaCalendarToDevice(); });
		}

		private void AddGoHejaCalendarToDevice()
		{
			try
			{
				NSError error;

				////remove existing descending events from now in "goHeja Events" calendar of device.
				var calendars = DeviceCalendar.Current.EventStore.GetCalendars(EKEntityType.Event);
				foreach (var calendar in calendars)
				{
					if (calendar.Title == PortableLibrary.Constants.DEVICE_CALENDAR_TITLE)
					{
						goHejaCalendar = calendar;

						EKCalendar[] calendarArray = new EKCalendar[1];
						calendarArray[0] = calendar;
						NSPredicate pEvents = DeviceCalendar.Current.EventStore.PredicateForEvents(NSDate.Now.AddSeconds(-(3600 * 10000)), NSDate.Now.AddSeconds(3600 * 10000), calendarArray);
						EKEvent[] allEvents = DeviceCalendar.Current.EventStore.EventsMatching(pEvents);
						if (allEvents == null)
							continue;
						foreach (var pEvent in allEvents)
						{
							NSError pE;
							DateTime now = DateTime.Now;
							DateTime startNow = new DateTime(now.Year, now.Month, now.Day);
							var startString = baseVC.ConvertDateTimeToNSDate(startNow);
							if (pEvent.StartDate.Compare(startString) == NSComparisonResult.Descending)
								DeviceCalendar.Current.EventStore.RemoveEvent(pEvent, EKSpan.ThisEvent, true, out pE);
						}
					}
				}

				if (goHejaCalendar == null)
				{
					goHejaCalendar = EKCalendar.Create(EKEntityType.Event, DeviceCalendar.Current.EventStore);
					EKSource goHejaSource = null;

					foreach (EKSource source in DeviceCalendar.Current.EventStore.Sources)
					{
						if (source.SourceType == EKSourceType.CalDav && source.Title == "iCloud")
						{
							goHejaSource = source;
							break;
						}
					}
					if (goHejaSource == null)
					{
						foreach (EKSource source in DeviceCalendar.Current.EventStore.Sources)
						{
							if (source.SourceType == EKSourceType.Local)
							{
								goHejaSource = source;
								break;
							}
						}
					}
					if (goHejaSource == null)
					{
						return;
					}
					goHejaCalendar.Title = PortableLibrary.Constants.DEVICE_CALENDAR_TITLE;
					goHejaCalendar.Source = goHejaSource;
				}

				DeviceCalendar.Current.EventStore.SaveCalendar(goHejaCalendar, true, out error);

				if (error == null)
					AddEvents();
			}
			catch (Exception e)
			{
				new UIAlertView("add events process", e.Message, null, "ok", null).Show();
			}
		}

		private void AddEvents()
		{
			var pastEvents = baseVC.GetPastEvents();
			var todayEvents = baseVC.GetTodayEvents();
			var futureEvents = baseVC.GetFutureEvents();

			AddEventsToGoHejaCalendar(pastEvents);
			AddEventsToGoHejaCalendar(todayEvents);
			AddEventsToGoHejaCalendar(futureEvents);
		}

		private void AddEventsToGoHejaCalendar(List<GoHejaEvent> eventsData)
		{
			if (goHejaCalendar == null || eventsData == null)
				return;

			foreach (var goHejaEvent in eventsData)
			{
				EKEvent newEvent = EKEvent.FromStore(DeviceCalendar.Current.EventStore);

				var startDate = goHejaEvent.StartDateTime();
				var endDate = goHejaEvent.EndDateTime();

				DateTime now = DateTime.Now;
				DateTime startNow = new DateTime(now.Year, now.Month, now.Day);
				DateTime startDay = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second);
				var deltaSec = (startDay - startNow).TotalSeconds;
				if (deltaSec < 0)
					continue;

				newEvent.StartDate = baseVC.ConvertDateTimeToNSDate(startDate);
				newEvent.EndDate = baseVC.ConvertDateTimeToNSDate(endDate);
				newEvent.Title = goHejaEvent.title;

				string eventDescription = baseVC.FormatEventDescription(goHejaEvent.eventData);

				string[] arryEventDes = eventDescription.Split(new char[] { '~' });

				for (var i = 0; i < arryEventDes.Length; i++)
				{
					newEvent.Notes += arryEventDes[i].ToString() + Environment.NewLine;
				}

				var strDistance = goHejaEvent.distance;
				var floatDistance = strDistance == "" ? 0 : float.Parse(strDistance);
				var b = Math.Truncate(floatDistance * 100);
				var c = b / 100;
				var formattedDistance = c.ToString("F2");

				var durMin = goHejaEvent.durMin == "" ? 0 : int.Parse(goHejaEvent.durMin);
				var durHrs = goHejaEvent.durHrs == "" ? 0 : int.Parse(goHejaEvent.durHrs);
				var pHrs = durMin / 60;
				durHrs = durHrs + pHrs;
				durMin = durMin % 60;

				var strDuration = durHrs.ToString() + ":" + durMin.ToString("D2");

				newEvent.Notes += Environment.NewLine + "Planned HB : " + goHejaEvent.hb + Environment.NewLine +
								"Planned Load : " + goHejaEvent.tss + Environment.NewLine +
								"Planned distance : " + formattedDistance + "KM" + Environment.NewLine +
								"Duration : " + strDuration + Environment.NewLine;

				//var encodedTitle = System.Web.HttpUtility.UrlEncode(goHejaEvent.title);

				//var urlDate = newEvent.StartDate;
				//var strDate = String.Format("{0:dd-MM-yyyy hh:mm:ss}", startDate);
				//var encodedDate = System.Web.HttpUtility.UrlEncode(strDate);
				//var encodedEventURL = String.Format(PortableLibrary.Constants.URL_EVENT_MAP, encodedTitle, encodedDate, AppSettings.Username);

				//newEvent.Url = new NSUrl(System.Web.HttpUtility.UrlEncode(encodedEventURL)); ;

				//add alarm to event
				EKAlarm[] alarmsArray = new EKAlarm[2];
				alarmsArray[0] = EKAlarm.FromDate(newEvent.StartDate.AddSeconds(-(60 * 45)));
				alarmsArray[1] = EKAlarm.FromDate(newEvent.StartDate.AddSeconds(-(60 * 60 * 12)));
				newEvent.Alarms = alarmsArray;

				newEvent.Calendar = goHejaCalendar;

				NSError e;
				DeviceCalendar.Current.EventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out e);
			}
		}

		public override void WillTerminate(UIApplication application)
		{
			if (bgThread != -1)
			{
				UIApplication.SharedApplication.EndBackgroundTask(bgThread);
			}
		}

		
	}

}


