namespace Infin8.Coapp.Dto
{
    public class PaySlipVM
    {
        //public Pay_Init PayInit { get; set; }
        //public Pay_Slip Payslip { get; set; }
        //public Pay_Att PayAtt { get; set; }
        //public  List<Pay_Slip_Tr>  PaySlipTrList { get; set; }
        //public List<Pay_Slip_Loan_Tr> PaySlipLoanTrList { get; set; }

        /// Pay Init
        public int Pay_Id { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }

        /// Pay att
        public int Att_HQ { get; set; }
        public int Att_CAMP { get; set; }
        public int Att_CL { get; set; }
        public int Att_EL { get; set; }
        public int Att_ML { get; set; }
        public int Att_FH { get; set; }
        public int Att_HD { get; set; }
        public int Att_LOP { get; set; }
        public int Att_TD { get; set; }

        /// Pay slip
        public double Pay_Basic { get; set; }
        public double Pay_Basic_Earned { get; set; }
        public double Pay_PP { get; set; }
        public double Pay_PP_Earned { get; set; }
        public double Pay_GradePay { get; set; }
        public double Pay_GradePay_Earned { get; set; }
        public double Pay_DA_Percent { get; set; }
        public double Pay_DA_Earned { get; set; }
        public double Pay_SLS { get; set; }
        public double Pay_ExGratia { get; set; }
        public double Pay_Bonus { get; set; }
        public double Pay_PF { get; set; }
        public double Pay_VPF { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }

        ///Pay slip  tr
        public int All_Id { get; set; }
        public int Ded_Id { get; set; }
        public int Loan_Id { get; set; }
        public int Led_Id { get; set; }
        public int All_Type { get; set; }
        public int Ded_Type { get; set; }
        public int All_Ded_Type { get; set; }
        public double All_Ded_Amt { get; set; }
        public double Allowance_Amt { get; set; }
        public double Deduction_Amt { get; set; }

        /// Pay all ded master
        public string? All_Name { get; set; }
        public string? Ded_Name { get; set; }


    }
}
