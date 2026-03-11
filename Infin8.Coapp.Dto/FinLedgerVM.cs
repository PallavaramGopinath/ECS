using System.ComponentModel.DataAnnotations;

namespace Infin8.Coapp.Dto
{
    public class FinLedgerVM
    {
        [Required(ErrorMessage = "Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Ledger Id must be greater than 0 (not selected)")]
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public int Fnl_Id { get; set; }
        public string? Fnl_Name { get; set; }
        public int Led_SlNo { get; set; }


    }
}
