using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Firebase.Messaging;

namespace goheja.Services
{
	[Service(Label = "FCMService")]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class FCMService : FirebaseMessagingService
	{
		public override void OnMessageReceived(RemoteMessage message)
		{
            var mData = message.Data;
			Debug.WriteLine("recipientID: " + mData["recipientID"]);
			Debug.WriteLine("senderName: " + mData["senderName"]);
            Debug.WriteLine("practiceId: " + mData["practiceId"]);
			Debug.WriteLine("practiceType: " + mData["practiceType"]);
            Debug.WriteLine("practiceName: " + mData["practiceName"]);
            Debug.WriteLine("practiceDate: " + mData["practiceDate"]);
            Debug.WriteLine("description: " + mData["description"]);
            Debug.WriteLine("osType: " + mData["osType"]);

            if (AppSettings.CurrentUser.isFcmOn)
			    SendNotification(mData);
		}

        void SendNotification(IDictionary<string, string> mData)
        {
            var msg =   mData["description"].Length > 100 ? mData["description"].Substring(0, 100) : mData["description"];

            var textStyle = new NotificationCompat.InboxStyle();
            textStyle.SetBigContentTitle("Notification from " + mData["senderName"]);
            textStyle.AddLine("Practice Type : " + mData["practiceType"]);
            textStyle.AddLine("Practice Name : " + mData["practiceName"]);
            textStyle.AddLine("Practice Date : " + mData["practiceDate"]);
            textStyle.AddLine(" ");
			textStyle.AddLine(msg);
            textStyle.SetSummaryText("Tap to open");

            var intent = new Intent(this, typeof(EventInstructionActivity));
            intent.PutExtra("FromWhere", "RemoteNotification");
            intent.PutExtra("SelectedEventID", mData["practiceId"]);
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0 /* Request code */, intent, PendingIntentFlags.OneShot);

            var defaultSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var notificationBuilder = new NotificationCompat.Builder(this)
                                                            .SetSmallIcon(Resource.Drawable.icon_remote_notification)
                                                            .SetStyle(textStyle)
                                                            .SetAutoCancel(true)
                                                            .SetSound(defaultSoundUri)
                                                            .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);

            notificationManager.Notify(0 /* ID of notification */, notificationBuilder.Build());
        }
	}
}
