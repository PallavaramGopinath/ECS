using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoPayPFData
    {
        public DateTime Transaction_Date { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public double PFBalance { get; set; }
        public double VPFBalance { get; set; }
        public double InterestOnPF { get; set; } /// including vpf balance
        public double EmployersPFBalance { get; set; }
        public double InterestOnEmployersPF { get; set; }
        public double TotalBalance => PFBalance + InterestOnPF + EmployersPFBalance + InterestOnEmployersPF;
        public double InterestOnPFCalculated { get; set; }
        public double InterestOnEmployersPFCalculated { get; set; }
        public DateTime? InterestAppliledDate { get; set; }
        public double PFReceived { get; set; }
        public  double VPFReceived { get; set; }
        public double EmployersPFReceived { get; set; }
        public double TotalReceived => PFReceived + VPFReceived + EmployersPFReceived;
        public double PFWithdrawn { get; set; }
        public double VPFWithdrawn { get; set; }
        public double EmployersPFWithdrawn { get; set; }
        public double InterestOnPFWithdrawn { get; set; }
        public double InterestOnEmployersPFWithdrawn { get; set; }
        public double TotalWidhdrawn => PFWithdrawn + VPFWithdrawn + EmployersPFWithdrawn + InterestOnPFWithdrawn   + InterestOnEmployersPFWithdrawn;
        public string? BrCode { get; set; }
        public decimal YrId { get; set; }
        public decimal Created_By { get; set; }
        public bool _isError { get; set; }
        public string? errorMessage { get; set; }
    }
}
