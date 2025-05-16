namespace Infin8.Coapp.BusinessLogic
{
    public interface IVerifyHandler
    {
        Boolean VerifyNomenClature(string accountNo, string accountType, int loanSchemeId, string brCode, out string errorMessage);
        Boolean VerifyDuplicateAccountNo(string accountNo, string tableName, string searchField, string deleteField);
    }
}
