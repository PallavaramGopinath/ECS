using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ISBCAMasterRepository
    {
        Task<(bool result,decimal accId, string accNo)> AddSBCAMasterAsync(SBCA_Master sbcaMaster);
        Task<bool> EditSBCAMasterAsync(SBCA_Master sbcaMaster);
        Task<List<DropdownItem>> GetSBCANosByMemIdAsync(decimal memId);
    }
}
