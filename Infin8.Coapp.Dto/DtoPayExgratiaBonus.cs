namespace Infin8.Coapp.Dto
{
    public class DtoPayExgratiaBonus
    {
        public DateTime Transaction_Date { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public decimal Pay_Id { get; set; }
        public double TotalPayment_Amount { get; set; }
        public double Amount { get; set; }
        public List<DtoBonusPaymentData>? Component_List { get; set; }
        public string? BrCode { get; set; }
        public decimal YrId { get; set; }
        public decimal Created_By { get; set; }
        public bool _isError { get; set; }
        public string? errorMessage { get; set; }
    }

    public class DtoBonusPaymentData
    {
        public decimal Employee_Id { get; set; }
        public double PaymentAmount { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        
    }
}
