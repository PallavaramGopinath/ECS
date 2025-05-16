
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Salary_Template
    {
        [Key]
        public int MST_Id { get; set; }
        public int MST_Type { get; set; }
        public string? MST_Name { get; set; }
        public int MSTLed_Id { get; set; }
        public string? MST_Notes { get; set; }
        public bool MST_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public bool IsSocietyDemand { get; set; }
        public bool IsBasicPay { get; set; }
        public string? BrCode { get; set; }
    }
}
