using System;
using System.Collections.Generic;

namespace PortableLibrary.Model
{
    public class FBNotificationContent
    {
        public string recipientID { get; set; }
        public string senderName { get; set; }
        public string practiceId { get; set; }
        public string practiceType { get; set; }
        public string practiceName { get; set; }
        public string practiceDate { get; set; }
        public string description { get; set; }
        public Constants.OS_TYPE osType { get; set; }
    }

    public class FBNotification
    {
        public FBNotificationContent data { get; set; }
        public string to { get; set; }
    }
}
