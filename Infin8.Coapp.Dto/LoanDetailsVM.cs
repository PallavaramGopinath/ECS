namespace Infin8.Coapp.Dto
{
    public class LoanDetailsVM
    {
        public decimal loanid { get; set; }
        public int schemeid { get; set; }
        public string? loanno { get; set; }
        public double roi { get; set; }
        public int Prl_Prd { get; set; }
        public int Int_Prd { get; set; }
        public double instalmentamt { get; set; }
        public string? schemename { get; set; }
        public DateTime? demanddate { get; set; }
        public decimal prlledid { get; set; }
        public decimal intledid { get; set; }
        public decimal iodledid { get; set; }
        public decimal piledid { get; set; }
        public int instalType { get; set; }
        public int intapplication { get; set; }
        public int piapplication { get; set; }
        public int iodapplication { get; set; }
        public int matchShareCapital { get; set; }
        public int adoptLoanLimit { get; set; }
        public double disbamt { get; set; }
        public double sanctionamt { get; set; }
        public DateTime sanctiondate { get; set; }
        public DateTime disbursementdate { get; set; }
        public double prlschedule { get; set; }
        public double prldemand { get; set; }
        public double prlcoll { get; set; }
        public double intcalulatedamt { get; set; }
        public DateTime? maxintcalcdate { get; set; }
        public double intcalcamt { get; set; }
        public DateTime? intcalcdate { get; set; }
        public double intcollamt { get; set; }
        public double picalulatedamt { get; set; }
        public DateTime? maxpicalcdate { get; set; }
        public double picalcamt { get; set; }
        public DateTime? picalcdate { get; set; }
        public double picollamt { get; set; }
        public double iodcalculatedamt { get; set; }
        public double iodcalcamt { get; set; }
        public double iodcollamt { get; set; }
        public DateTime trndate { get; set; }
        public double securityfacevalue { get; set; }
        public int maxtrnslno { get; set; }
        public DateTime? firstintduedate { get; set; }
        public DateTime? firstprlduedate { get; set; }
        public int StaffLoan_Int_Type { get; set; }
        public double prlOd { get; set; }
        public double intBal { get; set; }
        public double piBal { get; set; }
        public double iodBal { get; set; }
        public int Trn_SlNo { get; set; }
    }

}
