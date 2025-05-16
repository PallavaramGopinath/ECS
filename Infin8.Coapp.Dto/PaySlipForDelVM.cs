using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class PaySlipForDelVM
    {
        public PaySlipEmployeeVM? emp { get; set; }
        public Pay_Init? PayInit { get; set; }
        public Pay_Slip? Payslip { get; set; }
        public Pay_Att? PayAtt { get; set; }
        public List<PaySlipAllDedVM>? PaySlipAllDed { get; set; }
        //public List<PaySlipVM> PaySlipList { get; set; }
        public List<PaySlipLoanVM>? PaySlipLoanList { get; set; }
    }
}
