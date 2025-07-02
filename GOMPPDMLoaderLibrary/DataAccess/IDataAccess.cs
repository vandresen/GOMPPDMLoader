using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOMPPDMLoaderLibrary.DataAccess
{
    public interface IDataAccess
    {
        Task SaveData<T>(string connectionString, T data, string sql);
        Task<IEnumerable<T>> ReadData<T>(string sql, string connectionString);
        Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionString);
        Task BulkInsertAsync<T>(string connectionString, IEnumerable<T> data, string tableName);
    }
}
