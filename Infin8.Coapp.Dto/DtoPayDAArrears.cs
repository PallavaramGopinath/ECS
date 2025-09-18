using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoPayDAArrears
    {
        public decimal Pay_Id { get; set; }
        public DateTime Transaction_Date { get; set; }
        public DateTime DAFrom_Date { get; set; }
        public DateTime DATo_Date { get; set; }
        public double DARate { get; set; }
        public List<EmployeeMasterDto>? EmployeeList { get; set; }
        public List<PaySlipDAArrearsVM>? DAArrearsList { get; set; }
        public decimal Created_By { get; set; }
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
        public bool  IsError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
