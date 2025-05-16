using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMaxId
    {
        Task<decimal> GetMaxId(string tableName, string idColumn);
        Task<decimal> GetMaxIdWithBrCode(string tableName, string idColumn,string brCode);
    }
}
