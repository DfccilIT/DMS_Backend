using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using static ModuleManagementBackend.BAL.Services.AccountService;


namespace ModuleManagementBackend.API.ActionFilter
{

    public class SSOTokenValidateFilter : IAuthorizationFilter
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContext;
        //private readonly IMemoryCache cache; // 🆕 Add caching for performance
        private readonly ILogger<SSOTokenValidateFilter> logger; // 🆕 Add logging

        public SSOTokenValidateFilter(
            IConfiguration configuration,
            IHttpContextAccessor httpContext,

            ILogger<SSOTokenValidateFilter> logger)
        {
            this.configuration = configuration;
            this.httpContext = httpContext;

            this.logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is AllowAnonymousAttribute);
            if (allowAnonymous)
            {
                return;
            }


            var authorizationHeader = httpContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                SetUnauthorized(context, "No authorization header provided");
                return;
            }


            var token = authorizationHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
                        ? authorizationHeader.Substring("Bearer ".Length).Trim()
                        : null;

            // If no valid token, return unauthorized
            if (string.IsNullOrEmpty(token))
            {
                SetUnauthorized(context, "Invalid bearer token format");
                return;
            }


            bool isValidToken = false;
            string validationError = "Token validation failed";

            try
            {

                if (IsOIDCToken(token))
                {

                    var introspectionResult = ValidateTokenWithIntrospection(token);
                    isValidToken = introspectionResult.IsValid;
                    validationError = introspectionResult.Error ?? "Token introspection failed";
                }
                //else
                //{
                //    // Keep your existing validation logic for legacy tokens
                //    var tokenHandler = new JwtSecurityTokenHandler();
                //    var jwtToken = tokenHandler.ReadJwtToken(token);
                //    var SSOToken = Convert.ToString(jwtToken.Claims.FirstOrDefault(x => x.Type == "SSOToken")?.Value);
                //    var ssoToken = configuration["TokenKeysso"];

                //    if (!string.IsNullOrEmpty(SSOToken) && SSOToken.Trim() == ssoToken)
                //    {
                //        isValidToken = true;
                //    }
                //    else
                //    {
                //        int empCode = Convert.ToInt32(jwtToken.Claims.FirstOrDefault(x => x.Type == "EmpCode")?.Value == null ? "0" : jwtToken.Claims.FirstOrDefault(x => x.Type == "EmpCode")?.Value);
                //        isValidToken = ValidateLegacyToken(SSOToken, empCode);
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token validation exception for token: {TokenPrefix}", token.Substring(0, Math.Min(20, token.Length)));
                isValidToken = false;
                validationError = "Token validation exception";
            }


            if (!isValidToken)
            {
                SetUnauthorized(context, validationError);
                return;
            }


            logger.LogDebug("Token validation successful for request: {Path}", context.HttpContext.Request.Path);
        }

        private void SetUnauthorized(AuthorizationFilterContext context, string reason = "User is unauthorized")
        {
            logger.LogWarning("Unauthorized access attempt: {Reason}. Path: {Path}, IP: {IP}",
                reason,
                context.HttpContext.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress?.ToString());

            context.Result = new JsonResult(new
            {
                message = "User is unauthorized.",
                statusCode = StatusCodes.Status401Unauthorized,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        private bool IsOIDCToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);


                var issuer = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;
                var Environment = configuration["DeploymentModes"]?.ToString().Trim();
                var expectedIssuer = GetSSOBaseUrl(Environment);

                return issuer == expectedIssuer;
            }
            catch
            {
                return false;
            }
        }

        private (bool IsValid, string Error) ValidateTokenWithIntrospection(string token)
        {
            try
            {


                var Environment = configuration["DeploymentModes"]?.ToString().Trim();
                var BaseUrl = GetSSOBaseUrl(Environment);
                var introspectionUrl = $"{BaseUrl}/connect/introspect1";

                using var client = new HttpClient();
                var introspectionData = new FormUrlEncodedContent(new[]
                {
                 new KeyValuePair<string, string>("Token", token),

                });


                var response = client.PostAsync(introspectionUrl, introspectionData).Result;

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Token introspection failed with status: {StatusCode}", response.StatusCode);
                    return (false, "Introspection service unavailable");
                }

                var responseContent = response.Content.ReadAsStringAsync().Result;
                var introspectionResponse = JsonConvert.DeserializeObject<IntrospectionResponse>(responseContent);

                if (introspectionResponse?.Active != true)
                {
                    logger.LogWarning("Token introspection returned inactive token");
                    return (false, "Token is inactive or session expired");
                }


                var userInfo = new Root
                {
                    sub = introspectionResponse.Sub,
                    preferred_username = introspectionResponse.Username,
                    email = introspectionResponse.Email,
                    UserId = introspectionResponse.Username,
                    EmailId = introspectionResponse.Email,


                    UnitName = introspectionResponse.CustomClaims?.GetValueOrDefault("UnitName") ?? introspectionResponse.UnitName,
                    UnitId = introspectionResponse.CustomClaims?.GetValueOrDefault("UnitId") ?? introspectionResponse.UnitId,
                    Department = introspectionResponse.CustomClaims?.GetValueOrDefault("Department") ?? introspectionResponse.Department,
                    Designation = introspectionResponse.CustomClaims?.GetValueOrDefault("Designation") ?? introspectionResponse.Designation,

                    name = introspectionResponse.Username,
                    username = introspectionResponse.Username,
                    display_name = introspectionResponse.Username,
                    role = introspectionResponse.CustomClaims?.GetValueOrDefault("role"),
                    Level = introspectionResponse.CustomClaims?.GetValueOrDefault("Level"),
                    email_verified = "true"
                };

                httpContext.HttpContext.Items["CurrentUser"] = userInfo;

                logger.LogDebug("Token introspection successful for user: {UserId}", introspectionResponse.Username);
                return (true, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during token introspection");
                return (false, "Token validation failed");
            }
        }



        private string GetSSOBaseUrl(string environment)
        {
            return environment switch
            {
                "DFCCIL" => configuration["ApiBaseUrlsProd:SSO"],
                "DFCCIL_UAT" => configuration["ApiBaseUrlsDfcuat:SSO"],
                _ => configuration["ApiBaseUrlsCetpauat:SSO"]
            };
        }
    }

    public class IntrospectionResponse
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("client_id")]
        public string? ClientId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }


        [JsonProperty("exp")]
        public DateTime? Expiration { get; set; }

        [JsonProperty("iat")]
        public DateTime? IssuedAt { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("unit_name")]
        public string UnitName { get; set; }

        [JsonProperty("unit_id")]
        public string UnitId { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("custom_claims")]
        public Dictionary<string, string> CustomClaims { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }


    //public class SSOTokenValidateFilter : IAuthorizationFilter
    //{
    //    private readonly IConfiguration configuration;
    //    private readonly IHttpContextAccessor httpContext;

    //    public SSOTokenValidateFilter(IConfiguration configuration, IHttpContextAccessor httpContext)
    //    {
    //        this.configuration = configuration;
    //        this.httpContext = httpContext;
    //    }

    //    public void OnAuthorization(AuthorizationFilterContext context)
    //    {
    //        // Check if endpoint allows anonymous access
    //        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
    //            .Any(em => em is AllowAnonymousAttribute);
    //        if (allowAnonymous)
    //        {
    //            return; // Skip authorization for [AllowAnonymous] endpoints
    //        }

    //        // Get authorization header
    //        var authorizationHeader = httpContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

    //        // If no authorization header, return unauthorized
    //        if (string.IsNullOrEmpty(authorizationHeader))
    //        {
    //            SetUnauthorized(context);
    //            return;
    //        }

    //        // Extract token from Bearer header
    //        var token = authorizationHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
    //                    ? authorizationHeader.Substring("Bearer ".Length).Trim()
    //                    : null;

    //        // If no valid token, return unauthorized
    //        if (string.IsNullOrEmpty(token))
    //        {
    //            SetUnauthorized(context);
    //            return;
    //        }

    //        // Try to validate token
    //        bool isValidToken = false;

    //        try
    //        {
    //            // Check if this is an OIDC token from your SSO
    //            if (IsOIDCToken(token))
    //            {
    //                isValidToken = ValidateOIDCToken(token);
    //            }
    //            else
    //            {
    //                // Keep your existing validation logic for legacy tokens
    //                var tokenHandler = new JwtSecurityTokenHandler();
    //                var jwtToken = tokenHandler.ReadJwtToken(token);
    //                var SSOToken = Convert.ToString(jwtToken.Claims.FirstOrDefault(x => x.Type == "SSOToken")?.Value);
    //                var ssoToken = configuration["TokenKeysso"];

    //                if (!string.IsNullOrEmpty(SSOToken) && SSOToken.Trim() == ssoToken)
    //                {
    //                    isValidToken = true;
    //                }
    //                else
    //                {
    //                    int empCode = Convert.ToInt32(jwtToken.Claims.FirstOrDefault(x => x.Type == "EmpCode")?.Value == null ? "0" : jwtToken.Claims.FirstOrDefault(x => x.Type == "EmpCode")?.Value);
    //                    isValidToken = ValidateLegacyToken(SSOToken, empCode);
    //                }
    //            }
    //        }
    //        catch
    //        {
    //            isValidToken = false; // Any exception means invalid token
    //        }

    //        // If token validation failed, return unauthorized
    //        if (!isValidToken)
    //        {
    //            SetUnauthorized(context);
    //            return;
    //        }

    //        // If we reach here, token is valid - allow request to proceed
    //    }

    //    private void SetUnauthorized(AuthorizationFilterContext context)
    //    {
    //        context.Result = new JsonResult(new { message = "User is unauthorized.", statusCode = StatusCodes.Status401Unauthorized })
    //        {
    //            StatusCode = StatusCodes.Status401Unauthorized
    //        };
    //    }

    //    private bool IsOIDCToken(string token)
    //    {
    //        try
    //        {
    //            var tokenHandler = new JwtSecurityTokenHandler();
    //            var jwtToken = tokenHandler.ReadJwtToken(token);

    //            // Check if token has SSO issuer
    //            //var issuer = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;

    //            //var expectedIssuer = "https://app2.dfccil.com"; // Your SSO URL

    //            //return issuer == expectedIssuer;

    //            var issuer = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;
    //            var expectedIssuer = "https://app2.dfccil.com";

    //            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
    //            expectedIssuer = GetSSOBaseUrl(Environment);

    //            return issuer == expectedIssuer;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private bool ValidateOIDCToken(string token)
    //    {
    //        try
    //        {
    //            var tokenHandler = new JwtSecurityTokenHandler();
    //            var jwtToken = tokenHandler.ReadJwtToken(token);

    //            // Extract username for validation - try multiple claim types
    //            var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value ??
    //                          jwtToken.Claims.FirstOrDefault(x => x.Type == "username")?.Value ??
    //                          jwtToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value ??
    //                          jwtToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

    //            if (string.IsNullOrEmpty(username))
    //            {
    //                return false;
    //            }

    //            // Use your existing SSO validation endpoint
    //            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
    //            var BaseUrl = GetSSOBaseUrl(Environment);

    //            HttpClient client = new HttpClient();
    //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //            // Call the enhanced userinfo endpoint
    //            HttpResponseMessage response = client.GetAsync($"{BaseUrl}/connect/userinfo").Result;

    //            if (response.IsSuccessStatusCode)
    //            {
    //                string apiResponse = response.Content.ReadAsStringAsync().Result;

    //                // Use your working deserialization
    //                var responsedetails = JsonConvert.DeserializeObject<Root>(apiResponse);


    //                // Store user info for use in controllers
    //                httpContext.HttpContext.Items["CurrentUser"] = responsedetails;
    //                return true;

    //            }

    //            return false;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private bool ValidateLegacyToken(string ssoToken, int empCode)
    //    {
    //        try
    //        {
    //            if (string.IsNullOrEmpty(ssoToken) || empCode <= 0)
    //            {
    //                return false;
    //            }

    //            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
    //            var BaseUrl = GetSSOBaseUrl(Environment);

    //            HttpClient client = new HttpClient();
    //            client.BaseAddress = new Uri(BaseUrl + "/Login/IsValid?username=" + empCode + "&token=" + ssoToken + "");
    //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //            HttpResponseMessage response = client.PostAsync(client.BaseAddress, null).Result;

    //            if (response.IsSuccessStatusCode)
    //            {
    //                string apiResponse = response.Content.ReadAsStringAsync().Result;
    //                var responsedetails = JsonConvert.DeserializeObject<RootDfccil>(apiResponse);

    //                if (responsedetails != null && responsedetails.Status == "Success" && responsedetails.Employee != null)
    //                {
    //                    httpContext.HttpContext.Items["CurrentUser"] = responsedetails.Employee;
    //                    return true;
    //                }
    //            }

    //            return false;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private string GetSSOBaseUrl(string environment)
    //    {
    //        return environment switch
    //        {
    //            "DFCCIL" => configuration["ApiBaseUrlsProd:SSO"],
    //            "DFCCIL_UAT" => configuration["ApiBaseUrlsDfcuat:SSO"],
    //            _ => configuration["ApiBaseUrlsCetpauat:SSO"]
    //        };
    //    }
    //}
}
