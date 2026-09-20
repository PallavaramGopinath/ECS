using Infin8.Coapp.Dto;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ISuretyLoanSanctionHandler
    {
        Task<SuretyLoanSanctionDataVM> GetSuretyLoanMemberAsync(string memberNo, string brCode);
        Task<SuretyLoanSchemeDefaultsVM> GetSchemeDefaultsAsync(int schemeId, string brCode);
        Task<SuretyDeductionsDataVM> GetDeductionsDataAsync(decimal memId, decimal suretyMemId, int schemeId,
            double loanAmount, string brCode);

        /// <summary>Saves a sanction (eligibility + sanction header + trn lines) with the Sec 12 integrity guards.</summary>
        Task<SanctionSaveResultVM> SaveSanctionAsync(SuretyLoanSanctionSaveVM vm,
            decimal usrId, decimal yrId, string brCode, DateTime sanctionDate);
    }
}
