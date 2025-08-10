using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Json_Loan_Master
    {
        public int Scheme_Id { get; set; }
        public string? Application_No { get; set; }
        public DateTime? Application_Date { get; set; }
        public string? Resolution_No { get; set; }
        public DateTime? Resolution_Date { get; set; }
        public double Sanctioned_Amount { get; set; }
        public DateTime? Sanctioned_Date { get; set; }
        public int Principal_Period { get; set; }
        public int Interest_Period { get; set; }
        public DateTime? First_Principal_DueDate { get; set; }
        public DateTime? First_Interest_DueDate { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Penal_Rate { get; set; }
        public double Instalment_Amount { get; set; }
        public int Security_Type { get; set; }
        public double Security_Face_Value { get; set; }
        public double Drawing_Power { get; set; }
        
    }
}
