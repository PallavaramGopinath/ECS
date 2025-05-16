namespace Infin8.Coapp.Dto
{
    public class ViewJLIssuePLDB
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public int Loan_Id { get; set; }
        public string?    Scheme_Name { get; set; }
        public double RatePerGram { get; set; }
        public double GrossWeight { get; set; }
        public double Wastage { get; set; }
        public double NetWeight { get; set; }
        public double NetValue { get; set; }
        public DateTime San_Date { get; set; }
        public DateTime JL_DueDate { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; }
        public int Prl_Prd { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public string? JLO_Name { get; set; }
        public int JLO_Nos { get; set; }
    }
}
