using System;
using System.Collections.Generic;

namespace PortableLibrary.Model
{
    public class FCMDataNotification
    {
        public string senderId { get; set; }
        public string senderName { get; set; }
        public string practiceId { get; set; }
        public string commentId { get; set; }
        public string practiceType { get; set; }
        public string practiceName { get; set; }
        public string practiceDate { get; set; }
        public string description { get; set; }
        public Constants.OS_TYPE osType { get; set; }
    }

    public class FCMDisplayNotification
    {
        public FCMDisplayNotification(string title, string body)
        {
            this.title = title;
            this.body = body;
        }
        public string title { get; set; }
        public string body { get; set; }
    }

    public class FBNotification
    {
        public FBNotification()
        {
            content_available = true;
            priority = "high";
        }

		public List<string> registration_ids { get; set; }
        public FCMDataNotification data { get; set; }
        public FCMDisplayNotification notification { get; set; }

        public bool content_available { get; set; }
        public string priority { get; set; }
    }
}
