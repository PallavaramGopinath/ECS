namespace Infin8.Coapp.Dto
{
    public class MemberDetailsVM
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? FatherName { get; set; }
        public int MemberType { get; set; }
        public int? Age { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? DOR { get; set; }
        public string? GPF_No { get; set; }
        public string? TicketTokenGangNo { get; set; }
        public string? ReferName { get; set; }
        public DateTime? DOJ { get; set; }
        public double? BasicPay { get; set; }
        public bool IsMemExpired { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int? MemberStatus { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? Address { get; set; }
        public   string? BrCode { get; set; }
    }
}
