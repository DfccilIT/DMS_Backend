﻿using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.Model.DTOs.TokenDTO;

namespace ModuleManagementBackend.API.Controllers
{
    public class BaseController : ControllerBase
    {
        public TokenDetail TokenDetails { get; set; }
        public string SourceType { get; set; }

        public BaseController(IHttpContextAccessor httpContext)
        {

            var authorizationHeader = httpContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();


            SourceType = httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {

                var token = authorizationHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
                            ? authorizationHeader.Substring("Bearer ".Length).Trim()
                            : null;

                if (!string.IsNullOrEmpty(token))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    string getName = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
                    string empCodeString = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
                    string getDesignation = jwtToken.Claims.FirstOrDefault(x => x.Type == "Designation")?.Value;
                    string getUnit = jwtToken.Claims.FirstOrDefault(x => x.Type == "UnitName")?.Value;
                    string getUnitID = jwtToken.Claims.FirstOrDefault(x => x.Type == "UnitId")?.Value;
                    string getLavel = jwtToken.Claims.FirstOrDefault(x => x.Type == "Level")?.Value;
                    string getDepartment = jwtToken.Claims.FirstOrDefault(x => x.Type == "Department")?.Value;

                    if (int.TryParse(empCodeString, out int getEmpCode))
                    {
                        TokenDetails = new TokenDetail()
                        {
                            EmpCode = getEmpCode,
                            Department = getDepartment,
                            Designation = getDesignation,
                            Lavel = getLavel,
                            Unit = getUnit,
                            UnitID = getUnitID,
                        };
                    }
                    else
                    {
                        throw new Exception("Invalid EmpCode format in token.");
                    }
                }
            }
        }
    }
}
