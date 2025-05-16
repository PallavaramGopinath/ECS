namespace Infin8.Coapp.Dto
{
    public class rptFALedgerTrn
    {
        public decimal Led_Id { get; set; }
        public int Led_SlNo { get; set; }
        public string? Led_Name { get; set; }
        public int Grp_SlNo { get; set; }
        public string? Grp_Name { get; set; }
        public int Fnl_Id { get; set; }
        public string? Fnl_Name { get; set; }
        public double OB_Amt { get; set; }
        public double Tot_Rpt_Amt { get; set; }
        public double Tot_Pmt_Amt { get; set; }
        public double CB_Amt { get; set; }
    }
}
