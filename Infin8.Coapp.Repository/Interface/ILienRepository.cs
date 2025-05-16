using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ILienRepository
    {
        Task<bool> AddLienAsync(Lien lien);
        Task<bool> EditLienAsync(Lien lien);
    }
}
