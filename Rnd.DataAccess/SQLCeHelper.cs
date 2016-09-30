using System;
using System.Data;
using System.Data.SqlServerCe;

namespace Rnd.DataAccess
{
    public class SQLCeHelper:SQLCeMapper
    {
        
        public SQLCeHelper()
        {
            var _conn = SqlCeConnection();
        }

        public bool Result;

        /// <summary>
        /// Use for SELECT statement
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable ExecuteAdapter(string query)
        {
            var retValue = new DataTable();
            try
            {
                if (string.IsNullOrEmpty(query)) return null;
                
                var con = SqlCeConnection();

                con.Open();
                var cmd = new SqlCeCommand(query, con);

                var sqlceda = new SqlCeDataAdapter(cmd);

                sqlceda.Fill(retValue);

                con.Close();

                Result = true;
                return retValue;
            }
            catch (Exception)
            {
                Result = false;
            }

            return retValue;
        }

        /// <summary>
        /// Use for INSERT, UPDATE and DELETE statement
        /// </summary>
        /// <param name="query"></param>
        public  void ExecuteScalar(string query)
        {
            var con = SqlCeConnection();
            try
            {
                Object retval = 0;

                con.Open();
                var cmd = new SqlCeCommand(query, con);
                retval = cmd.ExecuteScalar();

                con.Close();
            }
            catch (Exception)
            {
                con.Close();
                throw;
            }

        }

   
    }
}
