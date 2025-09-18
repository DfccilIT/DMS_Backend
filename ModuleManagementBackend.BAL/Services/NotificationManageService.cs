using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

public class NotificationManageService:INotificationManageService
{
    private readonly IConfiguration configuration;
    private readonly SAPTOKENContext context;
    private readonly IDapperService dapper;

    public NotificationManageService(IConfiguration configuration, SAPTOKENContext _context, IDapperService _dapper)
    {
        this.configuration = configuration;
        context = _context;
        dapper = _dapper;
    }

    public async Task<ResponseModel> SendSMSUsingURL(string clientId, string appId, string smstext, string mobile, string templateId, string userid)
    {
        string environment = configuration["DeploymentModes"] ?? string.Empty;
        string smsUrl = configuration["SMSSettingsProd:SMSUrl"] ?? string.Empty;
        string senderId = configuration["SMSSettingsProd:SenderId"] ?? "DFCCIL";
        string smsKey = configuration["SMSSettingsProd:SMSKey"] ?? string.Empty;
        string entityId = configuration["SMSSettingsProd:Entityid"] ?? string.Empty;

        try
        {
            
            var isValid = context.apiUsers
                .Any(x => x.apiUserName.ToLower().Trim() == clientId.ToLower().Trim() && x.status == 0);

            if (!isValid)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "Invalid client."
                };
            }

            var apiCredit = context.apiUsersCredits
                .FirstOrDefault(x => x.apiUserName == clientId && x.Type == "SMS");

            if (apiCredit == null)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "No SMS credits found for client."
                };
            }

           
            int remainingSms = apiCredit.SMSLimit.GetValueOrDefault() - apiCredit.SMSConsumed.GetValueOrDefault();
            if (remainingSms <= 0)
            {
                return new ResponseModel
                {
                    Message = "SMS limit reached",
                    DataLength = remainingSms
                };
            }

            
            double usagePercent = (double)apiCredit.SMSConsumed.GetValueOrDefault() / apiCredit.SMSLimit.GetValueOrDefault() * 100;
            bool thresholdReached = usagePercent >= apiCredit.thresholdPercentage;
            bool canNotify = apiCredit.lastNotificationDate == null ||
                             (DateTime.Now - apiCredit.lastNotificationDate.Value).TotalDays >= 1;

            if (thresholdReached && canNotify)
            {
                await SMSThresholdWhatsappAsync(
                    new[] { "9306155597", "9896668009", "9582889598" },
                    remainingSms,
                    clientId);

                apiCredit.lastNotificationDate = DateTime.Now;
                apiCredit.notificationSent = true;
                await context.SaveChangesAsync();
            }

            
            if (string.Equals(environment, "local", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "SMS skipped in DEVELOPMENT mode.",
                    Data = new { Mobile = mobile, TemplateId = templateId }
                };
            }

            if (string.Equals(environment, "DFCCIL_UAT", StringComparison.OrdinalIgnoreCase))
            {
                mobile = configuration["SMSServiceDefaultNumber"] ?? string.Empty;
            }

            
            string requestUrl = $"{smsUrl}?key={smsKey}&senderid={senderId}&entityid={entityId}" +
                                $"&mobiles={mobile}&sms={Uri.EscapeDataString(smstext)}&tempid={templateId}";

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var response = await client.GetAsync(requestUrl);
            string responseContent = await response.Content.ReadAsStringAsync();

            
            if (response.IsSuccessStatusCode)
            {
                await InsertSmsManagementLog(clientId, appId, templateId, mobile, smstext, "SMS", userid);
                await InsertSmsLogDetails(mobile, smstext, userid, responseContent + " , SMS Sent Successfully");

                apiCredit.SMSConsumed += 1;
                await context.SaveChangesAsync();

                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "SMS sent successfully.",
                    Data = new { Mobile = mobile, TemplateId = templateId }
                };
            }
            else
            {
                await InsertSmsLogDetails(mobile, smstext, userid, "SMS not sent: " + responseContent);

                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "SMS not sent.",
                    Data = new { Mobile = mobile, TemplateId = templateId, Response = responseContent }
                };
            }
        }
        catch (Exception ex)
        {
            await InsertSmsLogDetails(mobile, smstext, userid, "Message Send Fail with error: " + ex.Message);

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Error sending SMS.",
                Data = new { Error = ex.Message }
            };
        }
    }
    public async Task<ResponseModel> SendWhatsAppSMS(string clientId, string appId,string templateId, string phoneNumber,List<string> variables, string createdBy)
    {
        try
        {
            var apiUser = await context.apiUsers
                .FirstOrDefaultAsync(x => x.apiUserName.ToLower().Trim() == clientId.ToLower().Trim() && x.status == 0);

            if (apiUser == null)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "User Not Authentic",
                    Data = null
                };
            }

            var apiCredit = await context.apiUsersCredits
                .FirstOrDefaultAsync(x => x.apiUserName == clientId && x.Type == "Whatsapp");

            if (apiCredit != null)
            {
                double usagePercent = (double)apiCredit.SMSConsumed!.Value / apiCredit.SMSLimit!.Value * 100;
                bool thresholdReached = usagePercent >= apiCredit.thresholdPercentage;
                bool canNotify = apiCredit.lastNotificationDate == null ||
                                 (DateTime.Now - apiCredit.lastNotificationDate.Value).TotalDays >= 1;

                int remSms = apiCredit.SMSLimit.Value - apiCredit.SMSConsumed.Value;

                if (thresholdReached && canNotify)
                {
                    await SMSThresholdWhatsappAsync(
                        new string[] { "9306155597", "9896668009", "9582889598" },
                        remSms,
                        $"{clientId} Whatsapp SMS"
                    );

                    apiCredit.lastNotificationDate = DateTime.Now;
                    apiCredit.notificationSent = true;
                    await context.SaveChangesAsync();
                }

                if (remSms == 0)
                {
                    return new ResponseModel
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "SMS Limit Reached",
                        Data = new { ClientId = clientId, Remaining = 0 }
                    };
                }
            }


            var result = await HawaWhatsAppSMS(templateId, phoneNumber, variables.ToArray());

            if (apiCredit != null)
            {
                apiCredit.SMSConsumed += 1;
                await context.SaveChangesAsync();
            }


            await InsertSmsManagementLog(
                clientId: clientId,
                appId: appId,
                templateId: templateId,
                phoneEmail: phoneNumber,
                variables: string.Join(",", variables),
                SeviceType: "WhatsApp",
                createdBy: createdBy
            );

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "WhatsApp SMS sent successfully",
                Data = new { ClientId = clientId, Template = templateId, Phone = phoneNumber, Result = result }
            };
        }
        catch (Exception ex)
        {
            return new ResponseModel
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Error sending WhatsApp SMS",
                Data = new { Error = ex.Message }
            };
        }
    }
    public async Task<ResponseModel> SendEmailAsync( string subject, string bodyHtml, List<string> toEmails,string clientId, string appId, string createdBy,List<string>? ccEmails = null, List<string>? bccEmails = null)
    {
        var response = new ResponseModel();

        try
        {
            if (toEmails == null || !toEmails.Any())
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "At least one recipient email is required."
                };
            }
            
            string environment = configuration["DeploymentModes"] ?? string.Empty;
            if (string.Equals(environment, "DFCCIL_UAT", StringComparison.OrdinalIgnoreCase))
            {
                toEmails= new List<string>();
                ccEmails=null;
                bccEmails=null;
                toEmails.Add("Saurabhc519@gmail.com");
            }

            var allEmails = toEmails
                .Concat(ccEmails ?? Enumerable.Empty<string>())
                .Concat(bccEmails ?? Enumerable.Empty<string>())
                .Distinct()
                .ToList();

            var invalidEmails = allEmails.Where(e => !IsValidEmail(e)).ToList();
            if (invalidEmails.Any())
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Invalid email address(es): {string.Join(", ", invalidEmails)}"
                };
            }

           
            var emailConfig = await context.mstEmailsConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppId == appId);

            if (emailConfig == null)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"No email configuration found for AppId: {appId}"
                };
            }

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            using (var client = new SmtpClient(emailConfig.SmtpServer, emailConfig.Port))
            {
                client.EnableSsl = emailConfig.EnableSsl ?? false;
                client.Credentials = new NetworkCredential(emailConfig.SenderEmail, emailConfig.Password);

                using (var message = new MailMessage())
                {
                   
                    if (!string.IsNullOrWhiteSpace(emailConfig.DisplayName))
                        message.From = new MailAddress(emailConfig.SenderEmail, emailConfig.DisplayName);
                    else
                        message.From = new MailAddress(emailConfig.SenderEmail);

                    foreach (var to in toEmails.Distinct())
                        message.To.Add(to);

                    if (ccEmails != null)
                    {
                        foreach (var cc in ccEmails.Distinct())
                            message.CC.Add(cc);
                    }

                    if (bccEmails != null)
                    {
                        foreach (var bcc in bccEmails.Distinct())
                            message.Bcc.Add(bcc);
                    }

                    message.Subject = subject;
                    message.Body = bodyHtml;
                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                }
            }

            
            foreach (var email in allEmails)
            {
                var smsLog = new EmailSmsManagement
                {
                    ClientId = clientId,
                    AppId = appId,
                    ServiceType = "Email",
                    TemplateId = "",
                    Phone_Email = email,
                    ListOfVariable = bodyHtml,
                    CreatedBy = createdBy,
                    CreatedOn = DateTime.Now
                };

                context.EmailSmsManagements.Add(smsLog);
            }

            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Email sent successfully and logged per recipient.";
            response.Data = new
            {
                To = toEmails,
                Cc = ccEmails,
                Bcc = bccEmails,
                ClientId = clientId,
                AppId = appId,
                LoggedCount = allEmails.Count
            };
        }
        catch (Exception ex)
        {
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Message = "Failed to send email.";
            response.Error = true;
            response.ErrorDetail = ex;
        }

        return response;
    }


    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    private async Task InsertSmsManagementLog(string clientId, string appId, string templateId, string phoneEmail, string variables, string SeviceType, string createdBy)
    {
        var smsLog = new EmailSmsManagement
        {
            ClientId = clientId,
            AppId = appId,
            TemplateId = templateId,
            Phone_Email = phoneEmail,
            ListOfVariable = variables,
            ServiceType=SeviceType,
            CreatedBy = createdBy,
            CreatedOn = DateTime.Now
        };

        context.EmailSmsManagements.Add(smsLog);
        await context.SaveChangesAsync();
    }
    private async Task InsertSmsLogDetails(string mobile, string smsText, string modifiedBy, string responseText)
    {
        await using var connection = new SqlConnection(context.Database.GetConnectionString());
        await using var cmd = new SqlCommand("InsertSmsLogDetails", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@MobileNumber", mobile);
        cmd.Parameters.AddWithValue("@SMSText", smsText);
        cmd.Parameters.AddWithValue("@UserId", modifiedBy);
        cmd.Parameters.AddWithValue("@ResponseText", responseText);

        await connection.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task<ResponseModel> SMSThresholdWhatsappAsync(string[] phoneNumbers, int smsRemaining, string clientName)
    {
        try
        {
            var url = configuration["WhatsappUrl"]??string.Empty;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", "4e460fd0-a725-11ef-bb5a-02c8a5e042bd");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            foreach (var originalPhoneNumber in phoneNumbers)
            {
                string phoneNumber = originalPhoneNumber.Length == 10 ? "91" + originalPhoneNumber : originalPhoneNumber;

                string jsonPayload = $@"{{
                ""messaging_product"": ""whatsapp"",
                ""recipient_type"": ""individual"",
                ""to"": ""{phoneNumber}"",
                ""type"": ""template"",
                ""template"": {{
                    ""name"": ""smsthresholdlhawa"",
                    ""language"": {{ ""code"": ""en_GB"" }},
                    ""components"": [
                        {{
                            ""type"": ""body"",
                            ""parameters"": [
                                {{ ""type"": ""text"", ""text"": ""{clientName}"" }},
                                {{ ""type"": ""text"", ""text"": ""{smsRemaining}"" }}
                            ]
                        }}
                    ]
                }}
            }}";

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        StatusCode = response.StatusCode,
                        Message = "Failed to send threshold WhatsApp message",
                        Data = new { Phone = phoneNumber, Client = clientName }
                    };
                }
            }

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Threshold WhatsApp notifications sent successfully",
                Data = new { Client = clientName, Remaining = smsRemaining }
            };
        }
        catch (Exception ex)
        {
            return new ResponseModel
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Error sending WhatsApp threshold notifications",
                Data = new { Error = ex.Message }
            };
        }
    }
    public async Task<ResponseModel> HawaWhatsAppSMS(string templateName, string phoneNumber, string[] args)
    {
        try
        {
            var url = configuration["WhatsappUrl"]??string.Empty;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", "4e460fd0-a725-11ef-bb5a-02c8a5e042bd");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            string parametersJson = string.Join(",", args.Select(val => $@"{{ ""type"": ""text"", ""text"": ""{val}"" }}"));

            if (phoneNumber.Length == 10)
                phoneNumber = "91" + phoneNumber;

            string jsonPayload = $@"{{
            ""messaging_product"": ""whatsapp"",
            ""recipient_type"": ""individual"",
            ""to"": ""{phoneNumber}"",
            ""type"": ""template"",
            ""template"": {{
                ""name"": ""{templateName}"",
                ""language"": {{ ""code"": ""en"" }},
                ""components"": [
                    {{
                        ""type"": ""body"",
                        ""parameters"": [
                            {parametersJson}
                        ]
                    }}
                ]
            }}
        }}";

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel
                {
                    StatusCode = response.StatusCode,
                    Message = "Failed to send WhatsApp SMS",
                    Data = new { Template = templateName, Phone = phoneNumber }
                };
            }

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "WhatsApp SMS sent successfully",
                Data = new { Template = templateName, Phone = phoneNumber, Variables = args }
            };
        }
        catch (Exception ex)
        {
            return new ResponseModel
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Error sending WhatsApp SMS",
                Data = new { Error = ex.Message }
            };
        }
    }

}
