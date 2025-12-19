using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ShippingComp.DAL
{
    public class DAL
    {
        //0 get connection string
        string connectionString = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;

        //1 define sql connection
        SqlConnection con;

        public DAL()
        {
            con = new SqlConnection(connectionString);
        }
    }
}