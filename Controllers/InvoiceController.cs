using ShippingComp.Models;
using ShippingComp.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShippingComp.Controllers
{
    public class InvoiceController : Controller
    {
        //0 get connection string
        string connectionString = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;

        //1 define sql connection
        SqlConnection con;

        public InvoiceController()
        {
            con = new SqlConnection(connectionString);
        }

        // GET: Client
        public ActionResult Index()
        {
            //ADO Disconnected Mode (in memory data manipulation, then save to actual DB)
            //1 define sql connection (done in constructor above)

            //2 define command
            SqlCommand cmd = new SqlCommand("select * from invoices", con);

            //3 definde data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            //4 define data table (in memory table)
            DataTable dt = new DataTable();

            //5 fill data
            da.Fill(dt);

            //6 use data
            return View(dt);
        }

        public ActionResult FilterByClient(string clientName)
        {
            //test
            //      https://localhost:44364/invoice/FilterByClient?clientname=abc

            SqlCommand cmd = new SqlCommand("select * from GetInvoicesForClient(@clientName)",con); //using SQL function
            cmd.Parameters.AddWithValue("@clientName", clientName);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            DataTable invoices = new DataTable();

            invoices.Load(dr);

            con.Close();

            return PartialView("_InvoicesTable", invoices);
            //return View("Index", invoices);
        }

        //[Route("invoice/gg")]
        [HttpGet]
        public ActionResult Add(InvoiceWithClientListViewModel vm)
        {
            SqlCommand cmd = new SqlCommand("select name,id from clients", con);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);

            vm.clientsList = dt;

            return View("Add",vm);
        }

        //[HttpPost]
        //public ActionResult Add(Invoice invFromReq)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //SqlCommand cmd = new SqlCommand();

        //        ////use stored procedure

        //        //con.Open();

        //        //cmd.ExecuteNonQuery();

        //        //con.Close();

        //        return RedirectToAction("Index");
        //    }

        //    return View("Add", invFromReq);
        //}

        public ActionResult GetShipmentsForClient(int cid)
        {
            SqlCommand cmd = new SqlCommand("select * from GetShipmentsForClient(@cid)", con);
            cmd.Parameters.AddWithValue("@cid", cid);

            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable shipments = new DataTable();
            shipments.Load(dr);
            con.Close();

            return PartialView("_ShipmentsDropDownList",shipments);
        }
    }
}