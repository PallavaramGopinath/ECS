using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class NewAccountNoHandler : INewAccountNoHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public NewAccountNoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<string> NewLoanNoAsync(int schemeId, string brCode)
        {
            //errorMessage = string.Empty;
            return await _unitOfWork.NewAccountNo.NewLoanNoAsync(schemeId, brCode);
        }

        public async Task<string> NewMemberNoAsync(int memType, string brCode)
        {
            return await _unitOfWork.NewAccountNo.NewMemberNoAsync(memType, brCode);
        }

        public async Task<string> NewSBNoAsync(int schemeId, string brCode)
        {
            return await _unitOfWork.NewAccountNo.NewSBNoAsync(schemeId, brCode);
        }

        public async Task<(string newSBNo, string errorMessage)> NewSBNoAsyncWithErrorMessage(int schemeId, string brCode)
        {
            return await _unitOfWork.NewAccountNo.NewSBNoAsyncWithErrorMessage(schemeId, brCode);

        }

        public async Task<string> NewTDNoAsync (int schemeId, string brCode)
        {
            return await _unitOfWork.NewAccountNo.NewTDNoAsync(schemeId, brCode);
        }
    }
}
