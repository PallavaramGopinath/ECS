namespace Infin8.Coapp.Dto
{
    /// <summary>Members' details for income tax (report 111). ITMembersTSIS.rdlc uses only these three fields.</summary>
    public class rptITMember
    {
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public int MemberType { get; set; }
    }
}
