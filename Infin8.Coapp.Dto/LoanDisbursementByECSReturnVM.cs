namespace Infin8.Coapp.Dto
{
    public class LoanDisbursementByECSReturnVM
    {
        public int LoanSanction_Id { get; set; }
        public DateTime SanctionDate { get; set; }
        public double LoanAmount { get; set; }
        public int RptMem_Id { get; set; }
        public int PmtLoan_Id { get; set; }
        public int RptLoan_Id { get; set; }
        public int RptDM_Id { get; set; }
        public int RptLed_Id { get; set; }
        public int ReceiptItem { get; set; }
        public int CalcAmount { get; set; }
        public int PrlLed_Id { get; set; }

        //public int SurityMem_Id { get; set; }
        //public string MemberNo { get; set; }
        //public string PerNo { get; set; }
        //public string MemberName { get; set; }
        //public int Scheme_Id { get; set; }
        //public string Scheme_Name { get; set; }
        //public string Loan_No { get; set; }
        
        
        //public int TotalReceiptAmount { get; set; }
        //public double NetAmount { get; set; }
        //public string BankName { get; set; }
        //public bool StopDemand { get; set; }
    }
}
