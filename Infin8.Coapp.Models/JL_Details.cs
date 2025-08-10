
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class JL_Details
    {
        [Key]
        public decimal JL_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double RatePerGram { get; set; }
        public double GrossWeight { get; set; }
        public double Wastage { get; set; }
        public double NetWeight { get; set; }
        public double NetValue { get; set; }
        public bool JL_Oe { get; set; }
        public bool JL_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? JewelsImagePath { get; set; }
        public double MarketRatePerGram { get; set; }
        public double EligiblePercentageOnMarketValue { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
