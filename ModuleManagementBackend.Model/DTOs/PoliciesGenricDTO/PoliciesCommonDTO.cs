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


        public class AddPolicyItemDto
        {
            public int FkPolId { get; set; }
            public string ItemSubject { get; set; }
            public string ItemType { get; set; }
            public string ItemContent { get; set; }
            public string ItemDescription { get; set; }
            public DateTime? OfficeOrderDate { get; set; }
            public double? OrderFactor { get; set; }

           
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
           
            public IFormFile? Doc { get; set; }
        }

        public class PolicyDto
        {
            public int pkPolId { get; set; }
            public string PolicyHead { get; set; }
            public List<PolicyDto> Children { get; set; } = new List<PolicyDto>();
            public List<PolicyItemDto> PolicyItems { get; set; } = new List<PolicyItemDto>();
        }

        public class PolicyItemDto
        {
            public int pkPolItemId { get; set; }
            public string itemSubject { get; set; }
            public string itemContent { get; set; }
            public string itemDescription { get; set; }
            public string itemType { get; set; }
            public string docName { get; set; }
            public string fileName { get; set; }
            public string Url { get; set; }
            public double? OrderFactor { get; set; }
        }

    }

}

