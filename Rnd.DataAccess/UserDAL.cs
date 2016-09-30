using System.Data;

namespace Rnd.DataAccess
{
    public class UserDAL
    {
        
        
        public UserDAL()
        {
            
        }

        public DataTable GetRoleList()
        {
            var MySqlManager = new SQLCeHelper();
            return MySqlManager.ExecuteAdapter("SELECT * FROM Role;");
        }

        public DataTable GetModuleList()
        {
            var MySqlManager = new SQLCeHelper();
            return MySqlManager.ExecuteAdapter("SELECT * FROM Module;");
        }

        public  void Save()
        {
            var MySqlManager = new SQLCeHelper();
            //MySqlManager.ExecuteScalar(string.Format("INSERT INTO users(FirstName,LastName,MI,UserID,PW,Role) " +
            //                                                    "VALUES('{0}','{1}','{2}','{3}','{4}','{5}');", 
            //                                                    _userModel.FirstName, _userModel.LastName,_userModel.MI,_userModel.UserID,_userModel.PW,_userModel.Role));
        }

        public string GetUserRole(string userID)
        {
            var MySqlManager = new SQLCeHelper();
            DataTable dt = MySqlManager.ExecuteAdapter(string.Format("SELECT Role from users where UserID='{0}'", userID));

            foreach (DataRow row in dt.Rows)
            {
                return row["Role"].ToString();
            }

            return string.Empty;
        }

        public DataTable GetUserList()
        {
            var MySqlManager = new SQLCeHelper();
            return MySqlManager.ExecuteAdapter(string.Format("select FirstName as 'First Name',LastName as 'Last Name', UserID as 'User ID', PW as Password, Role from users;"));
        }

        public  void Edit()
        {
        
        }

        public  void Delete()
        {
        
        }

        public  void Search()
        {
        
        }
    }
}
