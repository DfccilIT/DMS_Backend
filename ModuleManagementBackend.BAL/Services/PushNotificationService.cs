using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;

namespace ModuleManagementBackend.BAL.Services
{
    public class PushNotificationService : IPushNotification
    {

        public PushNotificationService()
        {

            var rootPath = Directory.GetCurrentDirectory();

            string htmlFilePath = @"" + rootPath + @"\wwwroot\FirebaseConfig\vmsfirebase-d609c-firebase-adminsdk-fbsvc-2dc4373a4a.json";

            // Initialize Firebase Admin SDK with the service account key

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(htmlFilePath)
                });

            }
        }

        // Method to send a notification to a single device
        public async Task<ResponseModel> SendPushNotificationAsync(string deviceToken, string title, string body)
        {

            var message = new Message()
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var result = await messaging.SendAsync(message);
            ResponseModel responseModel = new ResponseModel();
            responseModel.StatusCode = System.Net.HttpStatusCode.OK;
            responseModel.Message = "Successfully sent message";
            responseModel.Data = result;

            return responseModel;

        }

        // Method to send a notification to multiple devices
        public async Task<ResponseModel> SendPushNotificationToMultipleDevicesAsync(string[] deviceTokens, string title, string body)
        {

            var message = new MulticastMessage()
            {
                Tokens = deviceTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.SendEachForMulticastAsync(message);

            ResponseModel responseModel = new ResponseModel();
            responseModel.StatusCode = System.Net.HttpStatusCode.OK;
            responseModel.Message = "Successfully sent message";

            return responseModel;


        }
    }
}
