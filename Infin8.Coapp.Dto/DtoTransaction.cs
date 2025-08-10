using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTransaction
    {
        public decimal Staging_Id { get; set; }
        public decimal Transacted_Member_Id { get; set; }
        public DateTime Transacted_Date { get; set; }
        public decimal Checked_By { get; set; }
        public int Cash_Or_Adjustment { get; set; }
        public decimal Account_Holder_Member_Id { get; set; }
        public decimal Ledger_Id { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public DateTime?  Cheque_Date { get; set; }
        public string? Cheque_No { get; set; }
        public string? Issue_Bank_Name { get; set; }
        public int Related_Account_Id { get; set; }
        public string? Related_Account_Data { get; set; }
        public string? BrCode { get; set; }
    }
}
