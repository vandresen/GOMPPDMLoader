using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace GOMPPDMLoaderLibrary.DataAccess
{
    public class DapperDataAccess : IDataAccess
    {
        private readonly int _commandTimeout;

        public DapperDataAccess(int commandTimeout = 30) // default 30s
        {
            _commandTimeout = commandTimeout;
        }


        public async Task BulkInsertAsync<T>(string connectionString, IEnumerable<T> data, string tableName)
        {
            using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(sqlConnection)
            {
                DestinationTableName = tableName,
                BulkCopyTimeout = 300 // Adjust as needed
            };
            var dataTable = ToDataTable(data);
            await bulkCopy.WriteToServerAsync(dataTable);
        }

        public async Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionString)
        {
            using IDbConnection cnn = new SqlConnection(connectionString);
            return await cnn.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<T>> ReadData<T>(string sql, string connectionString)
        {
            using IDbConnection cnn = new SqlConnection(connectionString);
            return await cnn.QueryAsync<T>(sql);
        }

        public async Task SaveData<T>(string connectionString, T parameters, string sql)
        {
            using IDbConnection cnn = new SqlConnection(connectionString);
            await cnn.ExecuteAsync(sql, parameters, commandTimeout: _commandTimeout);
        }

        private static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, propType);
            }

            foreach (var item in data)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null) ?? DBNull.Value;
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}
