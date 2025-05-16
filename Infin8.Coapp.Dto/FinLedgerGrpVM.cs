namespace Infin8.Coapp.Dto
{
    public  class FinLedgerGrpVM
    {
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public Nullable<int> Fnl_Id { get; set; }
        public bool Grp_Mapped { get; set; }
        public Nullable<int> Usr_Id { get; set; }
        public Nullable<int> Grp_SlNo { get; set; }
        public bool Grp_Delete { get; set; }
        public string? Fnl_Name { get; set; }
    }
}
