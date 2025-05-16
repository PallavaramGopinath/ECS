using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MaxId : Repository<mem_master>, IMaxId
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public MaxId(CSISContext context) : base(context)
        {
        }
        public async Task<decimal> GetMaxId(string tableName, string idColumn)
        {
            decimal maxId = 0;
            string sql = "";
            try
            {
                sql = @"SELECT Max(" + idColumn + ") FROM " + tableName;
                var data = await CSISContext.Database.SqlQueryRaw<decimal>(
                    sql).FirstAsync();

                if (data > 0) maxId = data;
            }
            catch (Exception)
            {
                throw;
            }
            return maxId;
        }

        public async Task<decimal> GetMaxIdWithBrCode(string tableName, string idColumn, string brCode)
        {
            decimal maxId = 0;
            string sql = "";
            try
            {
                sql = @"SELECT Max(" + idColumn + ") FROM " + tableName + " WHERE brCode = @brCode";
                var data = await CSISContext.Database.SqlQueryRaw<decimal>(
                    sql
                    , new NpgsqlParameter("@brCode", brCode)).FirstAsync();
                if (data == 0)
                {
                    switch (tableName)
                    {
                        case "mem_master":
                            break;
                        case "Mem_Trn":
                            break;
                        case "SBCA_Master":
                            break;
                        case "Loan_Master":
                            break;
                        case "Termdeposit_Master":
                            break;
                           

                    }
                    if (data > 0) maxId = data;
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException($"Account with ID  not found.");
            }
            return maxId;
        }
        
    }
}
