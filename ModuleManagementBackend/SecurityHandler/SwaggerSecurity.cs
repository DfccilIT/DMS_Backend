using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ModuleManagementBackend.API.SecurityHandler
{
    public class SwaggerSecurity
    {

        private readonly RequestDelegate next;
        public SwaggerSecurity(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                string authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                   
                    var header = AuthenticationHeaderValue.Parse(authHeader);
                    var inBytes = Convert.FromBase64String(header.Parameter);
                    var credentials = Encoding.UTF8.GetString(inBytes).Split(':');
                    var username = credentials[0];
                    var password = credentials[1];
                 
                    if (username.Equals("DFC321")
                      && password.Equals("DFC@321"))
                    {
                        await next.Invoke(context).ConfigureAwait(false);
                        return;
                    }
                }
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.WriteAsync((int)HttpStatusCode.Unauthorized + " Unauthorized").ToString();
            }
            else
            {
                await next.Invoke(context).ConfigureAwait(false);
            }
        }
    }
    public static class SwaggerSecurityExtensions
    {
        public static IApplicationBuilder UseSwaggerSecurity(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerSecurity>();
        }
    }
}
