namespace Infin8.Coapp.Dto
{
    public class ListForDeletionVM
    {
        public int Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime Voc_Date { get; set; }
        public double Voc_Amt { get; set; }
        public int Voc_Type { get; set; }
    }
}
