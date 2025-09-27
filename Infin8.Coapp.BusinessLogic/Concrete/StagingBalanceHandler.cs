using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class StagingBalanceHandler : IStagingBalanceHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IAccountsHandler _accountsHandler;
        public StagingBalanceHandler(IUnitOfWork unitOfWork, IAccountsHandler accountsHandler   ) 
        { 
            _unitOfWork = unitOfWork;
            _accountsHandler = accountsHandler;
        }
        public async Task<List<DtoAccountsBalance>> GetStagingBalance(decimal yrId, DateTime accountingDate, DateTime fromDate, DateTime toDate, decimal created_By, string brCode)
        {
            List<DtoAccountsBalance> balanceList = new();
            try
            {
                var result = await _unitOfWork.Accounts.UpdateLedgerBalance(yrId, fromDate, toDate, brCode);
                if(result == true)
                {
                    var resultPush = await  _unitOfWork.StagingBalance.GetStagingBalance(yrId ,accountingDate ,created_By ,brCode);
                    
                    if(resultPush != null && resultPush.Any())
                    {
                        balanceList = resultPush.ToList ();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return balanceList;
        }

        public async Task<bool> AddStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode)
        {
            return await _unitOfWork.StagingBalance.AddStagingBalance(yrId, accountingDate, created_By, brCode);
        }
    }
}
