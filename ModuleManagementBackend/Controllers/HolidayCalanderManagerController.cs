using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using static ModuleManagementBackend.BAL.Services.AccountService;
using static ModuleManagementBackend.Model.DTOs.HolidayCalenderDTO.HolidayCalenderCommonDTO;

namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayCalanderManagerController : ControllerBase
    {
        private readonly IModuleManagementService _holidayService;
        private readonly IHttpContextAccessor httpContext;

        public HolidayCalanderManagerController(IModuleManagementService holidayService, IHttpContextAccessor httpContext)
        {
            _holidayService = holidayService;
            this.httpContext=httpContext;
        }
        private string loginUserId
        {
            get
            {
                var currentUser = httpContext.HttpContext?.Items["CurrentUser"] as Root;
                var userId = currentUser?.UserId ?? "0";
                return userId;
            }
        }

        [HttpGet]
        [Route("GetAllHolidays")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetAllHolidays(
            int? unitId = null,
            string? holidayType = null,
            string? unitName = null)
        {
            var result = await _holidayService.GetAllHolidays(unitId, holidayType, unitName);
            return Ok(result);
        }

       
       
        [HttpGet]
        [Route("GetHolidaysByDateRange")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetHolidaysByDateRange(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? unitId = null)
        {
            var result = await _holidayService.GetHolidaysByDateRange(fromDate, toDate, unitId);
            return Ok(result);
        }

       
       
        [HttpPost]
        [Route("CreateHoliday")]
        public async Task<ActionResult<ResponseModel>> CreateHoliday([FromBody] CreateHolidayCalendarDto createHolidayDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Validation failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Data = ModelState
                });
            }

            var result = await _holidayService.CreateHoliday(createHolidayDto,loginUserId);
            return Ok(result);
        }

       
        [HttpPut]
        [Route("UpdateHoliday")]
        public async Task<ActionResult<ResponseModel>> UpdateHoliday([FromBody] UpdateHolidayCalendarDto updateHolidayDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Validation failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Data = ModelState
                });
            }

            var result = await _holidayService.UpdateHoliday(updateHolidayDto, loginUserId);
            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteHoliday/{holidayId}")]
        public async Task<ActionResult<ResponseModel>> DeleteHoliday(int holidayId, string? deletedBy = null)
        {
            var result = await _holidayService.DeleteHoliday(holidayId, deletedBy);
            return Ok(result);
        }

       
        [HttpPost]
        [Route("BulkCreateHolidays")]
        public async Task<ActionResult<ResponseModel>> BulkCreateHolidays([FromBody] List<CreateHolidayCalendarDto> holidays)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Validation failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Data = ModelState
                });
            }

            var result = await _holidayService.BulkCreateHolidays(holidays, loginUserId);
            return Ok(result);
        }
       
        [HttpGet]
        [Route("GetCurrentYearHolidays")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetCurrentYearHolidays(int? unitId = null)
        {
            var currentYear = DateTime.Now.Year;
            var fromDate = new DateTime(currentYear, 1, 1);
            var toDate = new DateTime(currentYear, 12, 31);

            var result = await _holidayService.GetHolidaysByDateRange(fromDate, toDate, unitId);
            return Ok(result);
        }

       
        [HttpGet]
        [Route("GetUpcomingHolidays")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetUpcomingHolidays(int? unitId = null)
        {
            var fromDate = DateTime.Now.Date;
            var toDate = DateTime.Now.Date.AddDays(30);

            var result = await _holidayService.GetHolidaysByDateRange(fromDate, toDate, unitId);
            return Ok(result);
        }

        
        [HttpGet]
        [Route("GetHolidaysByMonth")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetHolidaysByMonth(
            int year,
            int month,
            int? unitId = null)
        {
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            var result = await _holidayService.GetHolidaysByDateRange(fromDate, toDate, unitId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetGazettedHolidays")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetGazettedHolidays(
            int? unitId = null,
            string? unitName = null)
        {
            var result = await _holidayService.GetAllHolidays(unitId, "GH", unitName);
            return Ok(result);
        }

       
        [HttpGet]
        [Route("GetRestrictedHolidays")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel>> GetRestrictedHolidays(
            int? unitId = null,
            string? unitName = null)
        {
            var result = await _holidayService.GetAllHolidays(unitId, "RH", unitName);
            return Ok(result);
        }
    }
}