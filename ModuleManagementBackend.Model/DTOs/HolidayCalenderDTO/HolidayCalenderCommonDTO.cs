using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.HolidayCalenderDTO
{
    public class HolidayCalenderCommonDTO
    {
        public class HolidayCalendarDto
        {
            public int HolidayId { get; set; }
            public int SerialNumber { get; set; }
            public DateTime HolidayDate { get; set; }
            public string HolidayDescription { get; set; }
            public string HolidayType { get; set; }
            public string HolidayTypeName => HolidayType == "GH" ? "Gazetted Holiday" : "Restricted Holiday";
            public string DayOfWeek { get; set; }
            public int UnitId { get; set; }
            public string UnitName { get; set; }
            public bool IsActive { get; set; }
            public string? CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string? UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
        }

        public class CreateHolidayCalendarDto
        {

            [Required]
            public DateTime HolidayDate { get; set; }

            public string? HolidayDescription { get; set; }

            [Required]
            [StringLength(10)]
            public string HolidayType { get; set; } 

            [Required]
            [StringLength(20)]
            public string DayOfWeek { get; set; }

            [Required]
            public int UnitId { get; set; }

            [Required]
            [StringLength(100)]
            public string UnitName { get; set; }


        }

        public class CreateHolidayCalendarNewDto
        {

            [Required]
            public DateTime HolidayDate { get; set; }

            public string? HolidayDescription { get; set; }

            [Required]
            [StringLength(10)]
            public string HolidayType { get; set; }

            [Required]
            [StringLength(20)]
            public string DayOfWeek { get; set; }

            //[Required]
            //public int UnitId { get; set; }

            //[Required]
            //[StringLength(100)]
            //public string UnitName { get; set; }
            public List<UnitsDto> Units { get; set; }

        }
        public class UnitsDto
        {
            [Required]
            public int UnitId { get; set; }

            [Required]
            [StringLength(100)]
            public string UnitName { get; set; }
        }

        public class UpdateHolidayCalendarDto
        {
            [Required]
            public int HolidayId { get; set; }

            public DateTime HolidayDate { get; set; }

            [StringLength(255)]
            public string? HolidayDescription { get; set; }

            [StringLength(10)]
            public string HolidayType { get; set; }

            [StringLength(20)]
            public string DayOfWeek { get; set; }

            public int UnitId { get; set; }

            [StringLength(100)]
            public string UnitName { get; set; }
           
        }

        public class MasterHolidayCalendarSearchDto
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string? HolidayType { get; set; }
            public int? UnitId { get; set; }
            public string? UnitName { get; set; }
            public string? HolidayDescription { get; set; }
            public bool? IsActive { get; set; } = true;
        }
    }
}

