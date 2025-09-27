using System.Security.Principal;

namespace Infin8.Coapp.Dto
{
    public class MemberLedgerVM
    {
        public decimal Mem_Id { get; set; }
        public string? Led_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? Voc_No { get; set; }
        public decimal Led_Id { get; set; }
        public int Trn_Type { get; set; }
        public double? Rpt_Amt { get; set; }
        public double? Pmt_Amt { get; set; }
        public double? Bal_Amt { get; set; }
        public int? IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public int? IntPaid_Amt { get; set; }
        public int? IntBal_Amt { get; set; }
        public decimal Voc_Id  { get; set; }
        public int Trn_SlNo { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
