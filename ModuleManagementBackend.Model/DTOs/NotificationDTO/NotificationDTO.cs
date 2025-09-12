using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.NotificationDTO
{
    public class NotificationDTO
    {
        public class SendSMSRequest
        {
            public string SmsText { get; set; }
            public string Mobile { get; set; }
            public string UserId { get; set; }
            public string TemplateId { get; set; }
            public string ClientId { get; set; }
            public string AppId { get; set; }
        }

        public class SendWhatsAppRequest
        {
            public string ClientId { get; set; }
            public string AppId { get; set; }
            public string TemplateId { get; set; }
            public string PhoneNumber { get; set; }
            public List<string> Variables { get; set; }
            public string CreatedBy { get; set; }
        }
        public class EmailRequestDto
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public List<string> To { get; set; }
            public List<string>? Cc { get; set; }
            public List<string>? Bcc { get; set; }
            public string ClientId { get; set; }
            public string AppId { get; set; }
            //public string TemplateId { get; set; }
            public string CreatedBy { get; set; }
        }
    }
}
