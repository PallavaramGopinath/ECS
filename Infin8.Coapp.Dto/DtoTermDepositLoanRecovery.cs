using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositLoanRecovery
    {
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Transaction_Date { get; set; }
        public List<DtoTermDepositLoanBalance>? Loan_Balance_List { get; set; }
        public List<DtoLoanNo>? FDLoanNos { get; set; }
        public double Total_Interest_Balance { get; set; }
        public double Total_Principal_Balance { get; set; }
        public double Total_Balance { get; set; }
        public double Total_Interest_Collection { get; set; }
        public double Total_Principal_Collection { get; set; }
        public double Total_Collection { get; set; }
    }
}
