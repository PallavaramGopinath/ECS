
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;

    public partial class Reports_Master
    {
        [Key]
        public int Report_Id { get; set; }
        public int ReportGrp_Id { get; set; }
        public string? ReportName { get; set; }
        public string? ReportFileName { get; set; }
        public bool ReportDelete { get; set; }
        public string? FirstSignature { get; set; }
        public string? SecondSignature { get; set; }
        public string? ThirdSignature { get; set; }
        public string? BrCode { get; set; }

        public static explicit operator Reports_Master(Task<Reports_Master> v)
        {
            throw new NotImplementedException();
        }
    }
}
