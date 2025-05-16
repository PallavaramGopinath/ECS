using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILienHandler
    {
        Task<bool> AddLienAsync(Lien lien);
        Task<bool> EditLienAsync(Lien lien);
    }
}
