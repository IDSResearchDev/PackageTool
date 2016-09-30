using System;
using System.Data.SqlServerCe;

namespace Rnd.DataAccess
{
    public abstract class SQLCeMapper : IDisposable
    {

        protected readonly string SQLConnectionString = string.Format("Data Source={0};Persist Security Info=True", (object)@"|DataDirectory|\Model\TransmittalModel.sdf");
        protected SqlCeConnection _conn;
        protected SqlCeCommand _cmd;
        protected SqlCeDataReader _dr;
        protected SqlCeDataAdapter _sda;
        protected SqlCeTransaction _trx;

        protected SqlCeConnection SqlCeConnection()
        {
            
            if (_conn == null)
                _conn = new SqlCeConnection(SQLConnectionString);

            return _conn;
        }
        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }
    }
}
