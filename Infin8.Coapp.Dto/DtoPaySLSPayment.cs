using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoPaySLSPayment
    {
        public DateTime Transaction_Date { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public decimal Pay_Id { get; set; }
        public double BasicPay { get; set; }
        public double PerPay { get; set; }
        public double GradePay { get; set; }
        public double DAAmount { get; set; }
        public double TotalAmount => BasicPay + PerPay + GradePay + DAAmount;
        public List<DtoSLSComponent>? Component_List { get; set; }
        public decimal DAId { get; set; }
        public double DAPercentage { get; set; }
        public double SLSAmount { get; set; }
        public int SLSDays { get; set; } = 15;
        public bool IsFinalSettlement { get; set; }
        public string? BrCode { get; set; }
        public decimal YrId { get; set; }
        public decimal Created_By { get; set; }
        public bool _isError { get; set; }
        public string? errorMessage { get; set; }
    }
    
    public class DtoSLSComponent
    {
        public decimal Pay_Id { get; set; }
        public decimal DAId { get; set; }
        public double DAPercentage { get; set; }
        public decimal Component_Id { get; set; }
        public string? Component_Name { get; set; }
        public string? Component_Code { get; set; }
        public double Allowance_Amount { get; set; }
        
    }
}
