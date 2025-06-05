using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<DataTable> GetDataTable(string procedureName, params SqlParameter[] parameters)
        {
            DataTable resultTable = new DataTable();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using SqlDataAdapter adapter = new SqlDataAdapter(command);

            try
            {
                adapter.Fill(resultTable);
            }
            catch (Exception ex)
            {
                // Handle exception (log it, rethrow it, etc.)
                throw new ApplicationException("Database error: " + ex.Message, ex);
            }

            return resultTable;
        }
        public async Task<DataSet> GetDataSet(string procedureName, params SqlParameter[] parameters)
        {
            DataSet resultSet = new DataSet();

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await using SqlCommand command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using SqlDataAdapter adapter = new SqlDataAdapter(command);

            try
            {
                await connection.OpenAsync();

                // SqlDataAdapter.Fill(DataSet) is not async — it runs sync inside the async method
                adapter.Fill(resultSet);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error: " + ex.Message, ex);
            }

            return resultSet;
        }
    }
}
