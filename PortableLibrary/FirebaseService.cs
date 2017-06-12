using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using PortableLibrary.Model;

namespace PortableLibrary
{
	public static class FirebaseService
	{
		public static HttpClient _httpClient = new HttpClient();

        public static FirebaseClient _firebase = new FirebaseClient(Constants.URL_FBDB_BASE);

		public static async Task SendNotification(FBNotificationContent nContent, string to)
        {
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + Constants.FCM_SERVER_KEY);

            try
            {
                var recipientFCMUserToken = await GetFCMUserToken(nContent.recipientID, nContent.osType);

                if (recipientFCMUserToken == null) return;

                var objNotification = new FBNotification();
                objNotification.data = nContent;
                //objNotification.to = to;
                objNotification.to = recipientFCMUserToken;
				var jsonNotification = JsonConvert.SerializeObject(objNotification);

                StringContent content = new StringContent(jsonNotification, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Constants.URL_FCM_BASE, content);
                var result = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static async Task RegisterFCMUser(LoginUser user)
		{
			try
			{
				var fcmUsers = await _firebase.Child("FCMUsers").OrderByKey().OnceAsync<LoginUser>();
				foreach (var fcmUser in fcmUsers)
				{
                    if (fcmUser.Object.userId == user.userId && fcmUser.Object.osType == user.osType)
                    {
                        await _firebase.Child("FCMUsers").Child(fcmUser.Key).PutAsync(user);
                        Debug.WriteLine("FCMUser Updated.");
                        return;
                    }
				}

                await _firebase.Child("FCMUsers").PostAsync(user);
				Debug.WriteLine("new FCMUser added.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
		}

		public static async Task<string> GetFCMUserToken(string userID, Constants.OS_TYPE osType)
		{
			try
			{
                var users = await _firebase.Child("FCMUsers").OrderByKey().OnceAsync<LoginUser>();

				foreach (var objUser in users)
				{
                    var user = objUser.Object;
					if (user.userId == userID && user.osType == osType)
						return user.fcmToken;
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			return null;
		}
    }
}
