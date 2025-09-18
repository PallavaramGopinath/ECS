using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoSecurityDepositData
    {
        public decimal Employee_Id { get; set; }
        public string? Employee_No { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public int Age { get; set; }
        
        public decimal Td_Id { get; set; }
        public string? Td_No { get; set; }
        public decimal Yr_Id { get; set; }
        public double RateOfInterest { get; set; }
        public double Balance { get; set; }
        public string? Nominee_Name { get; set; }
        public int Nominee_Age { get; set; }
        public string? Nominee_Relationship { get; set; }
    }
}
