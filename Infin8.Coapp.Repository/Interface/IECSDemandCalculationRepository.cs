using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IECSDemandCalculationRepository
    {
        Task<List<decimal>> GetLoanIdsToCalculateECSDemand(DateTime trnDate, DateTime dueDate, int societyType,string brCode);
    }
}
