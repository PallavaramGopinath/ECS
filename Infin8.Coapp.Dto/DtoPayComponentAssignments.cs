using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoPayComponentAssignments
    {
        public decimal Id { get; set; }
        public decimal Employee_Id { get; set; }
        public decimal Component_Id { get; set; }
        public string? Component_Name { get; set; }
        public string? Component_Code { get; set; }
        public double Assigned_Value { get; set; }
        public double Current_Value { get; set; }
        public decimal Led_Id { get; set; }
        public double Maximum_Amount { get; set; }
        public int Component_Type { get; set; }
        public bool Is_DA_Applicable { get; set; }
        public int Display_Order { get; set; }
        public string? Calculation_Method { get; set; } 
        public string? Calculation_Basis { get; set; }
        public bool Is_Overridden { get; set; }
        public double Percentage { get; set; }
    }
}
