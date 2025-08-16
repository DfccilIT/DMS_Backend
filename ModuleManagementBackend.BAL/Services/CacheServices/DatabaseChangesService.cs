using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModuleManagementBackend.BAL.IServices.ICacheServices;
using ModuleManagementBackend.DAL.DapperServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.BAL.Services.CacheServices
{
    public class DatabaseChangesService:IDatabaseChangesService
    {
        private readonly string _connectionString;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DatabaseChangesService> _logger;
        private readonly IDapperService _dapper;

        public DatabaseChangesService(IConfiguration configuration, ICacheService cacheService, ILogger<DatabaseChangesService> logger, IDapperService dapperService)
        {
            _dapper = dapperService;
            _connectionString = _dapper.getCoonectionString();
            _cacheService = cacheService;
            _logger = logger;

        }

        public async Task<string> GetDataVersionAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var query = @"
                SELECT MAX(LastModified) as LastModified
                FROM (
                    SELECT MAX(ISNULL(Modify_Date,'1990/01/01')) as LastModified FROM MstEmployeeMaster
                ) t";

                var command = new SqlCommand(query, connection);
                var result = await command.ExecuteScalarAsync();

                return result?.ToString() ?? DateTime.UtcNow.Ticks.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data version");
                return DateTime.UtcNow.Ticks.ToString();
            }
        }

        public async Task InvalidateCacheOnChangeAsync(string tableName)
        {
            await _cacheService.RemoveByPatternAsync("dfccil_directory");
            _logger.LogInformation("Cache invalidated for table: {TableName}", tableName);
        }
    }
}

