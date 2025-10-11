using ModuleManagementBackend.Model.Common;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IPushNotification
    {
        Task<ResponseModel> SendPushNotificationAsync(string deviceToken, string title, string body);
        Task<ResponseModel> SendPushNotificationToMultipleDevicesAsync(string[] deviceTokens, string title, string body);
    }
}
