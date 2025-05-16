namespace Infin8.Coapp.Repository
{
    public interface IVerifyRepository
    {
        Boolean VerifyNomenClature(string accountNo, string accountType, int loanSchemeId, string brCode, out string errorMessage);
        Boolean VerifyDuplicateAccountNo(string accountNo, string tableName, string searchField, string deleteField);
    }
}
