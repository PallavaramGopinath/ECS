
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class JL_Ornments
    {
        [Key]
        public decimal JLO_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public string? JLO_Name { get; set; }
        public int JLO_Nos { get; set; }
        public double JLO_GWt { get; set; }
        public double JLO_Wastage { get; set; }
        public double JLO_NWt { get; set; }
        public double JLO_Value { get; set; }
        public bool JLO_OE { get; set; }
        public bool JLO_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
