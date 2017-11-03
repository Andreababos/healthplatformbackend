using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace RS.NetDiet.Therapist.DataModel
{
    public class NdSqlExecutor : IDisposable
    {
        private bool disposed = false;
        private SqlConnection connection;

        public NdSqlExecutor()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NetDiet"].ConnectionString);
        }

        public int NonQuery(string commandText)
        {
            int rowsAffected = 0;
            connection.Open();
            using (var command = new SqlCommand(commandText, connection))
            {
                rowsAffected = command.ExecuteNonQuery();
            }
            connection.Close();

            return rowsAffected;
        }

        public List<List<Dictionary<string, object>>> Reader(string commandText)
        {
            List<List<Dictionary<string, object>>> data = new List<List<Dictionary<string, object>>>();
            connection.Open();
            using (var command = new SqlCommand(commandText, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    int resultIndex = 0;
                    do
                    {
                        data.Add(new List<Dictionary<string, object>>());
                        while (reader.Read())
                        {
                            data[resultIndex].Add(new Dictionary<string, object>());
                            var lLast = data[resultIndex].Last();
                            for (int i = 0; i < reader.FieldCount; ++i)
                                lLast[reader.GetName(i)] = reader.GetValue(i);
                        }
                        ++resultIndex;
                    } while (reader.NextResult());
                }
            }
            connection.Close();

            return data;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                connection.Dispose();
            }

            disposed = true;
        }
    }
}
