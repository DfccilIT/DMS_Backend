using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using System.Data;
using System.Net;
using System.Net.Mail;
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

    public async Task<ResponseModel> SendSMSUsingURL( string clientId, string appId, string smstext, string mobile,string templateId, string userid)
    {
        string environment = configuration["DeploymentModes"] ?? string.Empty;
        string smsUrl = configuration["SMSSettingsProd:SMSUrl"] ?? string.Empty;
        string senderId = configuration["SMSSettingsProd:SenderId"] ?? "DFCCIL";
        string smsKey = configuration["SMSSettingsProd:SMSKey"] ?? string.Empty;
        string entityId = configuration["SMSSettingsProd:Entityid"] ?? string.Empty;

        try
        {
            string requestUrl = $"{smsUrl}?key={smsKey}&senderid={senderId}&entityid={entityId}&mobiles={mobile}&sms={Uri.EscapeDataString(smstext)}&tempid={templateId}";

            if (string.Equals(environment, "local", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = "SMS skipped in DEVELOPMENT mode.",
                    Data = new { Mobile = mobile, TemplateId = templateId }
                };
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var response = await client.GetAsync(requestUrl);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await InsertSmsManagementLog(clientId, appId, templateId, mobile, smstext,"SMS", userid);
                await InsertSmsLogDetails(mobile, smstext, userid, responseContent + " , SMS Send Successfully");

                return new ResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = "SMS sent successfully.",
                    Data = new { Mobile = mobile, TemplateId = templateId }
                };
            }
            else
            {
                await InsertSmsLogDetails(mobile, smstext, userid, "SMS not sent: " + responseContent);

                return new ResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
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
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
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
    public async Task<ResponseModel> SendEmailAsync(string subject,string bodyHtml, List<string> toEmails, string clientId, string appId,  string createdBy,List<string>? ccEmails = null, List<string>? bccEmails = null)
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

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var smtpServer = configuration["MailSettings:Server"] ?? string.Empty;
            var port = int.Parse(configuration["MailSettings:Port"] ?? "587");
            var senderEmail = configuration["MailSettings:SenderEmail"] ?? string.Empty;
            var password = configuration["MailSettings:Password"] ?? string.Empty;

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, password);

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail);

                   
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

            
            var smsLog = new EmailSmsManagement
            {
                ClientId = clientId,
                AppId = appId,
                ServiceType="Email",
                TemplateId = "",
                Phone_Email = string.Join(",", toEmails.Concat(ccEmails ?? new List<string>()).Concat(bccEmails ?? new List<string>())),
                ListOfVariable = bodyHtml,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now
            };

            context.EmailSmsManagements.Add(smsLog);
            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Email sent and logged successfully.";
            response.Data = new
            {
                To = toEmails,
                Cc = ccEmails,
                Bcc = bccEmails,
                ClientId = clientId,
                AppId = appId
                
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
