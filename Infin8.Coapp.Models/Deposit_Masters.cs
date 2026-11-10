namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Masters
    {
        [Key]
        public int Deposit_Id { get; set; }
        public string? Deposit_Name { get; set; }
        public int Led_Id { get; set; }
        public int Interest_Led_id { get; set; }
        public int PI_Led_Id { get; set; }
        public double Percentage { get; set; }
        public double Maximum_Amount { get; set; }
        public double Fixed_Amount { get; set; }
        public string? Frequency { get; set; }
        public int Demand_Month { get; set; }
        public bool Is_Active { get; set; }
        public double Percentage_Towards_Deposit { get; set; }
        public double Percentag_Towards_Other_Ledger { get; set; }
        public int Other_Led_Id { get; set; }
        public bool Is_Option_Allowed { get; set; }
        public int Is_Arrear_Demand_Applicable { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public int Interest_Calculation_Period { get; set; }
    }
}
