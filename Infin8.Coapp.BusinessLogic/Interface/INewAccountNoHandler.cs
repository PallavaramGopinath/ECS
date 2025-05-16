namespace Infin8.Coapp.BusinessLogic
{
    public interface INewAccountNoHandler
    {
        Task<string> NewMemberNoAsync(int memType, string brCode);
        Task<string> NewLoanNoAsync(int schemeId, string brCode);
        Task<string> NewTDNoAsync(int schemeId, string brCode);
        Task<(string newSBNo, string errorMessage)> NewSBNoAsyncWithErrorMessage(int schemeId, string brCode);
        Task<string> NewSBNoAsync(int schemeId, string brCode);
    }
}
