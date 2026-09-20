using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class GeneralRepository : Repository<Gen_Bank_Name>, IGeneralRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public GeneralRepository(CSISContext context) : base(context)
        {
        }
        public async Task<string> GetSocietyName(string brCode)
        {
            return await CSISContext.Gen_Bank_Name.Where(x => x.BrCode == brCode).Select(x => x.Bank_Name!).FirstAsync();
        }
        public async Task<Gen_Bank_Name> GetSocietyData(string brCode)
        {
            Gen_Bank_Name bank = new();
            try
            {
                var result = await CSISContext.Gen_Bank_Name.Where(x => x.BrCode == brCode).FirstOrDefaultAsync();
                if (result != null) bank = result;
            }
            catch (Exception ex)
            {

                Console.WriteLine( ex.Message +  " Error while fetching society data");
            }
            
            return bank;
        }
    }
}
