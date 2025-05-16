namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// SanId   |<Mem No  |<Per No  |<Name                       |<Scheme Name            |<Loan No        |<Sanction Date |>Loan Amount       |<Deductions     |<Net Amount    |<Scheme Id  |<MemId |<SurityMemId |<Stop Demand |<Bank  
    /// </summary>
    public class LoanDisbursementByECSVM
    {
        public int LoanSanction_Id { get; set; }
        public int Mem_Id { get; set; }
        public int SurityMem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public string? Loan_No { get; set; }
        public DateTime SanctionDate { get; set; }
        public double LoanAmount { get; set; }
        public double TotalReceiptAmount { get; set; }
        public double NetAmount { get; set; }
        public string? BankName { get; set; }
        public string? SBAccountNo { get; set; }
        public string? IFSCCode { get; set; }
        public bool StopDemand { get; set; }
    }
}
