using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ModuleManagementBackend.DAL.DapperServices
{
    public class DapperService : IDapperService
    {
        private readonly IConfiguration configuration;
        private string? Connectionstring = "";

        public DapperService(IConfiguration config)
        {
            configuration = config;
            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
            var connectstring = "";
            if (Environment == "DFCCIL")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSProd");
            }
            else if (Environment == "DFCCIL_UAT")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSUat");
            }
            else
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSLocal");
            }

            Connectionstring = connectstring;

        }
        public void Dispose()
        {

        }


        public async Task<T?> Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            return  await db.QuerySingleOrDefaultAsync<T>(sp, parms, commandType: commandType);
        }

        public async Task<IEnumerable<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            return await db.QueryAsync<T>(sp, parms, commandType: commandType);
        }

        public DbConnection GetDbconnection()
        {
            return new SqlConnection(Connectionstring);
        }

        public async Task<T?> Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            T? result;
            using IDbConnection db = new SqlConnection(Connectionstring);
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    result = await db.ExecuteScalarAsync<T>(sp, parms, commandType: commandType, transaction: tran);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

        public async Task Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            int result;
            using IDbConnection db = new SqlConnection(Connectionstring);
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    result = await db.ExecuteAsync(sp, parms, commandType: commandType, transaction: tran);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

        }
        public async Task<int> ExecuteAsync(string storedProcedure, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            return await db.ExecuteAsync(storedProcedure, parameters, commandType: commandType);
        }

    }
}