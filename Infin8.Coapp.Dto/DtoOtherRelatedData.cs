using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoOtherRelatedData
    {
        public decimal Member_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public string? Cheque_No { get; set; }
        public DateTime? Cheque_Date { get; set; }
        public string? Issue_Bank_Name { get; set; }
        public double Balance_Amount { get; set; }
        public int Fin_Id { get; set; }
        public decimal Cash_Led_Id { get; set; }
    }
}
