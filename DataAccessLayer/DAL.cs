using ShippingComp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ShippingComp.DataAccessLayer
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

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            SqlCommand cmd = new SqlCommand("select * from clients", con);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                var c = new Client()
                {
                    Id = (int)dr.GetValue(0),
                    Name = dr.GetValue(1).ToString(),
                    Address = dr.GetValue(2).ToString(),
                    Phone = dr.GetValue(3).ToString()
                };
                clients.Add(c);
            }
            con.Close();

            return clients;
        }

        public int ExecuteDMLquery(string queryText)
        {
            SqlCommand cmd = new SqlCommand(queryText,con);
            con.Open();
            int RowsAffected = cmd.ExecuteNonQuery();
            con.Close();
            return RowsAffected;
        }
    }
}