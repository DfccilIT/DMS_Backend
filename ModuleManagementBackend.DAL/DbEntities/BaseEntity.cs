using System.ComponentModel.DataAnnotations;

namespace ModuleManagementBackend.DAL.DbEntities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedSource { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdatedSource { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
