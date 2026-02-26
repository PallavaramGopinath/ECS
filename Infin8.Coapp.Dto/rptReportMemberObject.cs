using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class rptReportMemberObject
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public int ReportId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime AsOnDate { get; set; }
        public string? BrCode { get; set; }
        public decimal Yr_Id { get; set; }
        public double MinimumMemberShareCapital { get; set; }

        ///public bool IsAClassMember { get; set; }
        ///public bool IsAssociateMember { get; set; }
        ///public bool IsNonMember { get; set; }
        ///public bool IsCancelledMember { get; set; }
        ///public List<int> MemberTypeList { get; set; }
        ///public MemberStatus SelectedStatus { get; set; } = MemberStatus.Active;
        public int MemberStatus { get; set; } = 1;

    }

    public enum MemberStatus
    {
        Active = 1,
        Inactive = 2,
        Both = 3
    }
}
