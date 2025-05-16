namespace Infin8.Coapp.Dto
{
    public class rptNEFT_Tnscb
    {
        /// <summary>
        /// 13 columns for TNS Bank NEFT format
        /// </summary>
        public string? SocietyAccountNo { get; set; } /// Cloumn A -1
        public double PaymentAmount { get; set; } /// Cloumn B -2
        public string? SBAccountNo { get; set; } /// Cloumn C -3
        public int SlNo { get; set; } /// Cloumn D -4
        public double ColumnE { get; set; } /// Cloumn E -5 (value 0.00)
        public string? IFSCCode { get; set; } /// Cloumn F -6
        public string? SocietyName { get; set; } /// Cloumn G -7
        public string? PaymentLedger { get; set; } /// Cloumn H -8
        public string? MemberName { get; set; } /// Cloumn I -9 , Column J -10 Payment Ledger (repeat)
        public int ColumnK { get; set; } /// Cloumn K -11 (value = 11)
        public int ColumnL { get; set; } /// Cloumn L -12 (value = 10)
        public string? SocietyEmail { get; set; } /// Cloumn M -13
    }
}
