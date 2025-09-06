using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class Pay_Component_Assignments
    {
        [Key]
        public decimal Id { get; set; }
        public decimal   Employee_Id { get; set; }
        public decimal Component_Id { get; set; }
        public double Assigned_Value { get; set; }
        public DateTime Effective_Date { get; set; }
        public DateTime? End_Date { get; set; }
        public bool Is_Overridden { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public bool Is_Active { get; set; }
        public string? BrCode { get; set; }
        public double Percentage { get; set; }
        public double Maximum_Amount { get; set; }
    }
}
