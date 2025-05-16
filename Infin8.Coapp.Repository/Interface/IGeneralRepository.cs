using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IGeneralRepository
    {
        Task<string> GetSocietyName(string brCode);
    }
}
