using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketProgramming
{
    public sealed class DBConnectionManager
    {
        #region Singleton
        private readonly string connectionstring;

        private static DBConnectionManager instance;

        public static DBConnectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBConnectionManager();
                }
                return instance;
            }

        } 
        #endregion

        #region Ctor
        private DBConnectionManager()
        {
            connectionstring = "Server=FINIX_PC;Database=UserDirectoryDB;User Id=sa;Password=Password1;";//ConfigurationManager.ConnectionStrings["UserDirectoryDBEntities"].ConnectionString;
        }
        #endregion

        #region Public Methods
        public DataTable GetData(string strQry)
        {
            DataTable dt;
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand(strQry, con);
                cmd.CommandType = CommandType.Text;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                dt = new DataTable();
                sda.Fill(dt);
            }

            return dt;
        }

        public void ExecuteQuery(string strQry)
        {
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand(strQry, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}
