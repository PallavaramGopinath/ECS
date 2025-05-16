
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Reports_Group
    {
        [Key]
        public int ReportGrp_Id { get; set; }
        public string? ReportGroupName { get; set; }
        public bool ReportGroupDelete { get; set; }
        public string? BrCode { get; set; }
    }
}
