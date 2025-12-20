using ShippingComp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace ShippingComp.Controllers
{
    public class ShipmentController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;

        //1 define sql connection
        SqlConnection con;

        public ShipmentController()
        {
            con = new SqlConnection(connectionString);
        }


        // GET: Shipment
        public ActionResult Index()
        {
            //ADO Disconnected Mode (in memory data manipulation, then save to actual DB)

            //1 define sql connection (done in constructor above)

            //2 define command
            SqlCommand cmd = new SqlCommand("select * from shipments", con);

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
            return View(dt);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View("Add");
        }

        //[HttpPost]
        //public ActionResult AddWithoutInvoice(Shipment shFromReq)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        SqlCommand cmd = new SqlCommand("insert into shipments values(@tnum,@sdate,@w,@cid)", con);
        //        cmd.Parameters.AddWithValue("@tnum", shFromReq.TrackingNumber);
        //        cmd.Parameters.AddWithValue("@sdate", shFromReq.ShipmentDate);
        //        cmd.Parameters.AddWithValue("@w", shFromReq.Weight);
        //        cmd.Parameters.AddWithValue("@cid", shFromReq.ClientId);

        //        con.Open();

        //        cmd.ExecuteNonQuery();

        //        con.Close();

        //        return RedirectToAction("Index");
        //    }

        //    return View("Add", shFromReq);
        //}

        [HttpPost]
        public ActionResult Add(Shipment shFromReq)
        {
            if (ModelState.IsValid)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_AddShipmentAndInvoice";
                cmd.Parameters.AddWithValue("@trackingnumber", shFromReq.TrackingNumber);
                SqlParameter shipmentdateParameter = new SqlParameter("@shipmentdate", shFromReq.ShipmentDate);
                cmd.Parameters.Add(shipmentdateParameter);
                cmd.Parameters.Add("@weight", SqlDbType.Decimal).Value = shFromReq.Weight;
                cmd.Parameters.Add(new SqlParameter("@clientid", shFromReq.ClientId));

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                return RedirectToAction("Index");
            }

            return View("Add", shFromReq);
        }

    }
}