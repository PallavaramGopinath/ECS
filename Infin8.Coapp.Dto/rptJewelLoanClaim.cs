using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class rptJewelLoanClaim
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
        public double NetValue { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public string? ItemDetails { get; set; }
    }
}
