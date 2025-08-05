using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModuleManagementBackend.Model.Common;
using Microsoft.AspNetCore.Builder;

namespace ModuleManagementBackend.Logger.ExceptionHandler
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new AppExecption(context.Response.StatusCode, ex.Message, ex.InnerException == null ? "" : ex.InnerException.ToString())
                    : new AppExecption(context.Response.StatusCode, ex.Message, ex.InnerException == null ? "" : ex.InnerException.ToString());

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                var getRequestErrorPath = context.Request == null ? "" : context.Request.Path.Value;
                var getStackTrace = ex.StackTrace == null ? "" : ex.StackTrace;

                ErrorLogModel errorLog = new ErrorLogModel()
                {
                    CreateDateTime = DateTime.Now,
                    Details = response.Details,
                    StatusCode = response.StatuCode,
                    Message = response.Message,
                    RequestPath = getRequestErrorPath,
                    StackTrace = getStackTrace
                };



                await context.Response.WriteAsync(json);
            }
        }

       

    }
    public static class ExceptionHandlerExtensions
    {
        public static IApplicationBuilder ExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}