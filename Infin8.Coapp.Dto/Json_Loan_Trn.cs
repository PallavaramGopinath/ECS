using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Json_Loan_Trn
    {
        public decimal Loan_Id { get; set; }
        public DateTime? Trn_Date { get; set; }
        public DateTime? Disb_Date { get; set; }
        public DateTime? Due_Date { get; set; }
        public double Disbursement_Amount { get; set; }
        public double Principal_Demand { get; set; }
        public double Interest_Calculated { get; set; }
        public DateTime? Interest_Applied_Date { get; set; }
        public double IOD_Calculated { get; set; }
        public double Penal_Calcualted { get; set; }
        public DateTime? Penal_Applied_Date { get; set; }
        public double Penal_Collection { get; set; }
        public double IOD_Collection { get; set; }
        public double Interest_Collection { get; set; }
        public double Principal_Collection { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Penal_Rate { get; set; }
    }
}
