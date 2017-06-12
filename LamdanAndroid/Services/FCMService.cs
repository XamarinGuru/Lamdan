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
			Debug.WriteLine("practiceType: " + mData["practiceType"]);
            Debug.WriteLine("practiceName: " + mData["practiceName"]);
            Debug.WriteLine("practiceDate: " + mData["practiceDate"]);
            Debug.WriteLine("description: " + mData["description"]);
            Debug.WriteLine("osType: " + mData["osType"]);

			SendNotification(mData);
		}

        void SendNotification(IDictionary<string, string> mData)
        {
            var summaryText = "Practice Type : " + mData["practiceType"] + "\n" +
                "Practice Name : " + mData["practiceName"] + "\n" +
                "Practice Date : " + mData["practiceDate"] + "\n\n" +
                mData["description"];


            var textStyle = new NotificationCompat.BigTextStyle();
            textStyle.SetBigContentTitle("Notification from " + mData["recipientName"]);
            textStyle.BigText("Tap to open");
            textStyle.SetSummaryText(summaryText);

            var intent = new Intent(this, typeof(EventInstructionActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0 /* Request code */, intent, PendingIntentFlags.OneShot);

            var defaultSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var notificationBuilder = new NotificationCompat.Builder(this)
            .SetSmallIcon(Resource.Drawable.icon_notification)
            .SetStyle(textStyle)
            .SetAutoCancel(true)
            .SetSound(defaultSoundUri)
            .SetContentIntent(pendingIntent);

			var notificationManager = NotificationManager.FromContext(this);

			notificationManager.Notify(0 /* ID of notification */, notificationBuilder.Build());
        }
	}

	
}
