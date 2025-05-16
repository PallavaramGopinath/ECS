namespace Infin8.Coapp.Dto
{
    public class MemberTransactionVM
    {
        public string? Voc_No { get; set; }
        public byte TrnType { get; set; }
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
        public decimal MemId { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Pmt { get; set; }
        public double Rpt { get; set; }
        public double Balance { get; set; }

        /// the below properties relates to SB Account
        public int IntCalculatedAmt { get; set; }
        public DateTime? IntCalculatedDate { get; set; }
        public int IntPaid { get; set; }
        public int IntNowCalcAmt { get; set; }
        public DateTime? IntNowCalcDate { get; set; }
        public int IntBalance { get; set; }
        public decimal AccId { get; set; }
    }
}
