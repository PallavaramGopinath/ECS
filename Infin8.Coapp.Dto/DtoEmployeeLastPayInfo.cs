using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoEmployeeLastPayInfo
    {
        public decimal Pay_Id { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_No { get; set; }
        public string? Employee_Name { get; set; }
        public string? Employee_Designation { get; set; }
        public string? Payment_Status { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public string? Pay_Month_Year { get; set; }
        public bool Pmt { get; set; }
        public string? BrCode { get; set; }
    }
}
