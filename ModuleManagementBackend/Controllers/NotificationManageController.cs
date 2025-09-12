using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using static ModuleManagementBackend.Model.DTOs.NotificationDTO.NotificationDTO;

namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationManageController : ControllerBase
    {
        private readonly INotificationManageService notificationManage;

        public NotificationManageController( INotificationManageService notificationManage)
        {
            this.notificationManage=notificationManage;
        }

        //[HttpPost("SendSMS")]
        //public async Task<IActionResult> SendSMS([FromBody] SendSMSRequest request)
        //{
        //    var result = await notificationManage.SendSMSUsingURL(
        //        smstext: request.SmsText,
        //        mobile: request.Mobile,
        //        userid: request.UserId,
        //        templateId: request.TemplateId,
        //        clientId: request.ClientId,
        //        appId: request.AppId
        //    );

        //    return StatusCode((int)result.StatusCode, result);
        //}

        
        //[HttpPost("SendWhatsAppSMS")]
        //public async Task<IActionResult> SendWhatsAppSMS([FromBody] SendWhatsAppRequest request)
        //{
        //    var result = await notificationManage.SendWhatsAppSMS(
        //        clientId: request.ClientId,
        //        appId: request.AppId,
        //        templateId: request.TemplateId,
        //        phoneNumber: request.PhoneNumber,
        //        variables: request.Variables,
        //        createdBy: request.CreatedBy
        //    );

        //    return StatusCode((int)result.StatusCode, result);
        //}

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
        {
            var result = await notificationManage.SendEmailAsync(
                subject: request.Subject,
                bodyHtml: request.Body,
                toEmails: request.To,
                clientId: request.ClientId,
                appId: request.AppId,
                //templateId: request.TemplateId,
                createdBy: request.CreatedBy,
                ccEmails: request.Cc,
                bccEmails: request.Bcc
            );

            return Ok(result);
        }

        

    }
}
