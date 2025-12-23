using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
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
    public class PaymentController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;
        SqlConnection con;

        public PaymentController()
        {
            //1 connection
            con = new SqlConnection(connectionString);
        }

        public ActionResult Index()
        {
            //2 sql command
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select * from InvoicesAndAmountPaid; select id, name from Clients;";
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Connection = con;

            //3 open con
            con.Open();

            //4 execute command
            SqlDataReader dr = cmd.ExecuteReader();

            //5 use data 
            DataTable paymentsDT = new DataTable();
            DataTable clientsDT = new DataTable();

            //DataTable[] dataTables = new DataTable[] { new DataTable(), new DataTable() };

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(paymentsDT);
            dataSet.Tables.Add(clientsDT);

            dataSet.Load(dr, LoadOption.PreserveChanges, paymentsDT, clientsDT);


            //6 close con
            con.Close();

            PaymentsDataTable_With_ClientsListViewModel model = new PaymentsDataTable_With_ClientsListViewModel();
            model.Payments = dataSet.Tables[0];
            model.Clients = dataSet.Tables[1];

            return View(model);
        }
        public ActionResult FilterPaymentsByClient(string client_name)
        {
            //2 sql command
            SqlCommand cmd = new SqlCommand();
            if (client_name == "all" || client_name == null)
            {
                cmd.CommandText = "select * from InvoicesAndAmountPaid";
            }
            else
            {
                cmd.CommandText = "select * from FilterPaymentsByClient(@client_name)";
                cmd.Parameters.AddWithValue("client_name", client_name);
            }
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Connection = con;

            //3 open con
            con.Open();

            //4 execute command
            SqlDataReader dr = cmd.ExecuteReader();

            //5 
            DataTable clients = new DataTable();
            clients.Load(dr);

            //6
            con.Close();

            return PartialView("_PaymentsForCertainClient", clients);
        }

        

        public ActionResult AddPayment(Payment ajaxP)
        {
            SqlCommand insertPayment = new SqlCommand();
            insertPayment.CommandText = "insert into Payments values (@invoiceid,@amountpaid,@paymentdate)";
            insertPayment.Parameters.AddWithValue("@invoiceid",ajaxP.InvoiceId);
            insertPayment.Parameters.AddWithValue("@amountpaid",ajaxP.AmountPaid);
            insertPayment.Parameters.AddWithValue("@paymentdate",ajaxP.PaymentDate);

            insertPayment.Connection = con;

            con.Open();
            int result = insertPayment.ExecuteNonQuery();
            con.Close();

            if (result == 0)
                return null;
            else
                return RedirectToAction("FilterPaymentsByClient");
        }

        public JsonResult GetAllPaymentsForThisInvoice(int id)
        {
            SqlCommand cmd = new SqlCommand("select * from payments where invoiceid = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            SqlDataReader reader =  cmd.ExecuteReader();
            DataTable payments = new DataTable();
            payments.Load(reader);
            con.Close();

            return Json(payments);
        }

    }
}