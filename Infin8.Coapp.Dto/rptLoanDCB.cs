namespace Infin8.Coapp.Dto
{
    public class rptLoanDCB
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? PreAdd1 { get; set; }
        public string? PreAdd2 { get; set; }
        public string? PreAdd3 { get; set; }
        public string? PrePin { get; set; }

        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public double Disb_Amt { get; set; }
        public double ArrPrlDem { get; set; }
        public double CurrPrlDem { get; set; }

        public double TotalPrlDem { get; set; }
        public double ArrIntDem { get; set; }
        public double CurrIntDem { get; set; }
        public double TotalIntDem { get; set; }
        public double ArrIODDem { get; set; }
        public double CurrIODDem { get; set; }
        public double TotalIODDem { get; set; }
        public double ArrPIDem { get; set; }
        public double CurrPIDem { get; set; }
        public double TotalPIDem { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double IODColl_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public double PrlBal { get; set; }
        public double IntBal { get; set; }
        public double IODBal { get; set; }
        public double PIBal { get; set; }
        public double AdvPrlBefore { get; set; }
        public double AdvPrlDuring { get; set; }
        public double AdvPrl { get; set; }
        public double Recper { get; set; }

        public double Loan_OS { get; set; }
        public double  Due_To { get; set; }     

    }
}
