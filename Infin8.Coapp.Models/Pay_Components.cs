using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public enum PayCalculationType
    {
        Percentage,
        Fixed
    }
    public class Pay_Components
    {
        [Key]
        public decimal Component_Id { get; set; }
        public string? Component_Name { get; set; }
        public string? Component_Code { get; set; }
        public int Component_Type { get; set; }
        public decimal Led_Id { get; set; }
        public bool Is_DA_Applicable { get; set; }
        public bool Is_SL_Applicable { get; set; }
        public int Display_Order { get; set; }
        public string? Calculation_Method { get; set; } 
        public string? Calculation_Basis { get; set; }
        public double Maximum_Amount { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public decimal Checked_By { get; set; }
        public DateTime? Checked_Date { get; set; }
        public string? BrCode { get; set; }
    }
}
