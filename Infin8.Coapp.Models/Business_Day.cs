namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Business_Day
    {
        [Key]
        public decimal Calendar_Id { get; set; }
        public System.DateTime Calendar_Date { get; set; }
        public string? Calendar_Status { get; set; }
        public bool Calendar_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }

    public enum Status
    {
        NotProcessed = 'N',
        Holiday = 'H',
        DayBegin = 'B',
        DayEnd = 'E'
    };
}
