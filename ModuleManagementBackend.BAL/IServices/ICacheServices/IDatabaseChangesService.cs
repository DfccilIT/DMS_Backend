using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.BAL.IServices.ICacheServices
{
    public interface IDatabaseChangesService
    {
        Task<long> GetDataVersionAsync();

        Task<long> GetDataVersionForPolicyAsync();
        Task InvalidateCacheOnChangeAsync(string tableName);
    }
}
