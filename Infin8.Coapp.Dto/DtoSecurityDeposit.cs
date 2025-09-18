using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoSecurityDeposit
    {
        public DateTime Transaction_Date { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public decimal Td_Id { get; set; }
        public string? TD_No { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public double Rate_Of_Interest { get; set; }
        public DtoSecurityDepositData? SecurityDeposit { get; set; }
        public string? BrCode { get; set; }
        public decimal YrId { get; set; }
        public decimal Created_By { get; set; }
        public bool _isError { get; set; }
        public string? errorMessage { get; set; }
    }
}
