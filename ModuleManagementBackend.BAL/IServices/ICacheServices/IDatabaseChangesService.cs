using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.BAL.IServices.ICacheServices
{
    public interface IDatabaseChangesService
    {
        Task<string> GetDataVersionAsync();
        Task InvalidateCacheOnChangeAsync(string tableName);
    }
}
