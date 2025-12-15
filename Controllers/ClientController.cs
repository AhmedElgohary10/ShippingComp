using ShippingComp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web;
using System.Web.Mvc;

namespace ShippingComp.Controllers
{
    public class ClientController : Controller
    {
        //0 get connection string
        string connectionString = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;

        //1 define sql connection
        SqlConnection con;

        public ClientController()
        {
            con = new SqlConnection(connectionString);
        }

        // GET: Client
        public ActionResult Index()
        {
            //ADO Disconnected Mode (in memory data manipulation, then save to actual DB)

            //1 define sql connection (done in constructor above)

            //2 define command
            SqlCommand cmd = new SqlCommand("select * from clients", con);

            //3 definde data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            //4 define data table (in memory table)
            DataTable dt = new DataTable();

            /*DataTable structure:
                * dt.Columns => DataColumnCollection => .Add() .Remove() .Count
                   * dt.Columns[0] => DataColumn => .ColumnName .DataType

                * dt.Rows => DataRowCollection => .Add() .Remove() .Find() .Count
                   * dt.Rows[0] => DataRow => .ItemArray .IsNull() .Delete()
             */


            //5 fill data
            da.Fill(dt);

            //6 use data
            ViewBag.Clients = dt;

            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View("Add");
        }


        [HttpPost]
        public ActionResult Add(Client cFromReq)
        {
            if (cFromReq.Name is null || cFromReq.Address is null || cFromReq.Phone is null)
            {
                return View(cFromReq);
            }

            //1 define sql connection (done in constructor above)

            //Client c = new Client();
            //c.Name = clientFromReq.Name;
            //c.Address = clientFromReq.Address;
            //c.Phone = clientFromReq.Phone;

            //2 command
            SqlCommand cmd = new SqlCommand("insert into clients values(@n,@a,@p)", con);
            cmd.Parameters.AddWithValue("@n", cFromReq.Name);
            cmd.Parameters.AddWithValue("@a", cFromReq.Address);
            cmd.Parameters.AddWithValue("@p", cFromReq.Phone);

            //3 open con
            con.Open();

            //4 execute command
            //5 result data bind
            int rowsAffected = cmd.ExecuteNonQuery();

            //6 close connection
            con.Close();

            return RedirectToAction("Index");
        }

        public ActionResult GetById(int id)
        {
            //1 define sql connection (done in constructor above)

            //2 define command
            string query = "select * from clients where id=@id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);

            //3 open connection 
            con.Open();

            //4 execute commands
            SqlDataReader dr = cmd.ExecuteReader();

            //5.1 databind
            Client c1 = new Client();
            dr.Read();
            c1.Id = (int)dr["id"];
            c1.Name = dr["name"].ToString();
            c1.Address = dr["address"].ToString();
            c1.Phone = dr["phone"].ToString();

            //5.2 databind to data table
            DataTable dt = new DataTable();
            dt.Load(dr);
            /*DataTable structure:
                * dt.Columns => DataColumnCollection => .Add() .Remove() .Count
                   * dt.Columns[0] => DataColumn => .ColumnName .DataType

                * dt.Rows => DataRowCollection => .Add() .Remove() .Find() .Count
                   * dt.Rows[0] => DataRow => .ItemArray .IsNull() .Delete()
             */

            //6 close connection
            con.Close();

            return Content(c1.ToString());
        }

    }
}