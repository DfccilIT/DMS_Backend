using ModuleManagementBackend.Model.Common;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface INotificationManageService
    {
        Task<ResponseModel> SendSMSUsingURL(
           string clientId,
           string appId,
           string smstext,
           string mobile,
           string templateId,
           string userid);

        Task<ResponseModel> SendWhatsAppSMS(
            string clientId,
            string appId,
            string templateId,
            string phoneNumber,
            List<string> variables,
            string createdBy);

        Task<ResponseModel> SendEmailAsync(
        string subject,
        string bodyHtml,
        List<string> toEmails,
        string clientId,
        string appId,
        string createdBy,
        List<string>? ccEmails = null,
        List<string>? bccEmails = null
    );
    }
}

