using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MenuMainHandler : IMenuMainHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MenuMainHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MenuMain>> GetMenuStructureAsync(string brCode, string role)
        {
            return await _unitOfWork.MenuMain.GetMenuStructureAsync(brCode,role);
        }
    }
}
