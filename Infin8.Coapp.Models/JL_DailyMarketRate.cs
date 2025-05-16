
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class JL_DailyMarketRate
    {
        [Key]
        public decimal MarketRate_Id { get; set; }
        public System.DateTime MarketRate_Date { get; set; }
        public double MarketRate { get; set; }
        public bool MarketRate_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
