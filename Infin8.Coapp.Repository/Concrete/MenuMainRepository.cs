using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MenuMainRepository : Repository<Menu_Main>, IMenuMainRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MenuMainRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<MenuMain>> GetMenuStructureAsync(string brCode)
        {
            var menuStructure = new List<MenuMain>();
            try
            {
                var query =  from main in CSISContext.Menu_Main
                            where !main.Menu_Delete && main.Voc_Status == "V"
                            orderby main.Menu_Id
                            select new MenuMain
                            {
                                MenuId = main.Menu_Id,
                                MenuName = main.Menu_Name,
                                MenuIcon = main.Menu_Icon,
                                MenuDelete = main.Menu_Delete,
                                BrCode = main.BrCode,
                                VocStatus = main.Voc_Status,
                                SubMenus = (from sub in CSISContext.Menu_Sub
                                            where sub.Menu_Id == main.Menu_Id &&
                                                  !sub.SubMenu_Delete &&
                                                  sub.Voc_Status == "V"
                                            orderby sub.SubMenu_Id
                                            select new MenuSub
                                            {
                                                SubMenuId = sub.SubMenu_Id,
                                                MenuId = sub.Menu_Id,
                                                SubMenuName = sub.SubMenu_Name,
                                                SubMenuIcon = sub.SubMenu_Icon,
                                                FormName = sub.Form_Name,
                                                SubMenuDelete = sub.SubMenu_Delete,
                                                BrCode = sub.BrCode,
                                                VocStatus = sub.Voc_Status,
                                                Forms = (from form in CSISContext.Menu_Forms
                                                         where form.SubMenu_Id == sub.SubMenu_Id &&
                                                               !form.Form_Delete &&
                                                               form.Voc_Status == "V"
                                                         orderby form.Form_Id
                                                         select new MenuForm
                                                         {
                                                             FormId = form.Form_Id,
                                                             SubMenuId = form.SubMenu_Id,
                                                             FormMenu = form.Form_Menu,
                                                             FormName = form.Form_Name,
                                                             FormIcon = form.Form_Icon,
                                                             FormDelete = form.Form_Delete,
                                                             BrCode = form.BrCode,
                                                             VocStatus = form.Voc_Status
                                                         }).ToList()
                                            }).ToList()
                            };

                if(query != null && query.Any())
                    menuStructure =  await query.AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                Console.Write (ex.ToString());
            }
            return menuStructure;
        }
    }
}
