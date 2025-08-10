using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Json_TermDeposit_Master
    {
        public int TDScheme_Id { get; set; }
        public string? TDH_name { get; set; }
        public int  TDH_Age { get; set; }
        public int Mode_Of_Opration { get; set; }
        public DateTime?  Account_Opened_Date { get; set; }
        public DateTime? Value_Date { get; set; }
        public double Deposit_Amount { get; set; }
        public int Period_In_Months { get; set; }
        public int Period_In_Days { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Penal_Rate { get; set; }
        public bool Is_Discount_Rate { get; set; }
        public bool is_Compound_Interest { get; set; }
        public int Compound_Frequency { get; set; }
        public string? Nominee1_Name { get; set; }
        public int Nominee1_Age { get; set; }
        public string? Nominee1_Relationship { get; set; }
        public string? Nominee2_Name { get; set; }
        public int Nominee2_Age { get; set; }
        public string? Nominee2_Relationship { get; set; }
    }
}
