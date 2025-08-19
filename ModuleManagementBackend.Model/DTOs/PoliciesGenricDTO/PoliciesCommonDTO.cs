using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.PoliciesGenricDTO
{
    public class PoliciesCommonDTO
    {

       
        public class PolicyDto
        {
            public int PkPolId { get; set; }
            public string PolicyHead { get; set; }
            public int? ParentPolicyId { get; set; }
           
        }

        public class GETPolicyDto
        {
            public int PkPolId { get; set; }
            public string PolicyHead { get; set; }
            public int? ParentPolicyId { get; set; }
            public int Status { get; set; } = 0;
            public string CreateBy { get; set; }
            public DateTime? CreateDate { get; set; }
            public string ModifyBy { get; set; }
            public DateTime? ModifyDate { get; set; }


            public List<GETPolicyDto> Children { get; set; } = new List<GETPolicyDto>();
        }
        public class AddPolicyDto
        {
            public string PolicyHead { get; set; }
            public int? ParentPolicyId { get; set; } = 0;
            public string CreateBy { get; set; }
        }

        public class UpdatePolicyDto
        {
            public int PkPolId { get; set; }
            public string PolicyHead { get; set; }
            public int? ParentPolicyId { get; set; }
            public int Status { get; set; }
            public string ModifyBy { get; set; }
        }

        // =====================
        // POLICY ITEM DTOs
        // =====================

       

        public class AddPolicyItemDto
        {
            public int FkPolId { get; set; }
            public string ItemSubject { get; set; }
            public string ItemType { get; set; }
            public string ItemContent { get; set; }
            public string ItemDescription { get; set; }
            public DateTime? OfficeOrderDate { get; set; }
            public double? OrderFactor { get; set; }
            public string CreateBy { get; set; }

           
            public IFormFile? Doc { get; set; }
        }

        public class UpdatePolicyItemDto
        {
            public int PkPolItemId { get; set; }
            public string? ItemSubject { get; set; }
            public string? ItemType { get; set; }
            public string? ItemContent { get; set; }
            public string? ItemDescription { get; set; }
            public DateTime? OfficeOrderDate { get; set; }
            public double? OrderFactor { get; set; }
            public int Status { get; set; }
            public string? ModifyBy { get; set; }

            // Optional file update
            public IFormFile? Doc { get; set; }
        }
    }

}

