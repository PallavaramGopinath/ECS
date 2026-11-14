using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    /// <summary>Queries and id allocation for the ECS RD New Account component, acc 17 (spec 15). Never calls SaveChanges.</summary>
    public interface IEcsRdCreateRepository
    {
        /// <summary>RD schemes (type 'R', not deleted) of the branch, by name.</summary>
        Task<List<TermDeposit_Schemes>> GetRdSchemesAsync(string brCode);

        /// <summary>The branch's RD scheme <paramref name="schemeId"/>, or null when missing, deleted or not an RD scheme.</summary>
        Task<TermDeposit_Schemes?> GetRdSchemeAsync(int schemeId, string brCode);

        /// <summary>Non-deleted ROI rows of the given schemes in the branch (all period types; the calculator filters 'M').</summary>
        Task<List<TermDeposit_Roi_Template>> GetRoiRowsAsync(IEnumerable<int> schemeIds, string brCode);

        /// <summary>Member by id, branch-filtered, with the status flags of §7.3.</summary>
        Task<DtoEcsRdMember?> GetMemberByIdAsync(decimal memId, string brCode);

        /// <summary>Members by id, branch-filtered.</summary>
        Task<Dictionary<decimal, DtoEcsRdMember>> GetMembersByIdAsync(IEnumerable<decimal> memIds, string brCode);

        /// <summary>Member by member number, branch-filtered (latest admission if duplicated).</summary>
        Task<DtoEcsRdMember?> GetMemberByNoAsync(string memberNo, string brCode);

        /// <summary>Nominees of the member's latest non-deleted RD in the branch, or null (§4.3).</summary>
        Task<DtoNominee?> GetLastRdNomineeAsync(decimal memId, string brCode);

        /// <summary>Every TD_No of the branch beginning with <paramref name="prefix"/>, deleted rows included (§5.3).</summary>
        Task<List<string?>> GetTdNosWithPrefixAsync(string prefix, string brCode);

        /// <summary>
        /// §10.5: allocates TD_Id (global MAX+1, including rows already tracked) and TDMem_Id (MAX+1 per holder), sets
        /// them on the entities and adds them to the change tracker. Returns the TD_Id.
        /// </summary>
        Task<decimal> AddRdAsync(TermDeposit_Master master, List<TermDeposit_Members> members);
    }
}
