namespace Infin8.Coapp.Dto
{
    public class PaySlipGeneratedForDeletion
    {
        public int Pay_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberName { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public double Pay_Net { get; set; }
    }
}
