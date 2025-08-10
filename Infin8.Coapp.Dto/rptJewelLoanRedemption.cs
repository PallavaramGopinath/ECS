using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanRedemption
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime Disb_Date { get; set; }
        public double Disb_Amount { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public int FullyCollectedNo { get; set; }
        public double FullyCollected { get; set; }
        public int PartiallyCollectedNo { get; set; }
        public double PartiallyCollected { get; set; }

    }
}
