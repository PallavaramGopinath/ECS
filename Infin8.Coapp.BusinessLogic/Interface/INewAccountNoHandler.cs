namespace Infin8.Coapp.BusinessLogic
{
    public interface INewAccountNoHandler
    {
        Task<string> NewMemberNoAsync(int memType, string brCode);
        Task<string> NewLoanNoAsync(long schemeId, string brCode);
        Task<string> NewTDNoAsync(long schemeId, string brCode);
        Task<(string newSBNo, string errorMessage)> NewSBNoAsyncWithErrorMessage(long schemeId, string brCode);
        Task<string> NewSBNoAsync(long schemeId, string brCode);
    }
}
