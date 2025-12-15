using ShippingComp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShippingComp.ViewModels
{
    public class InvoiceWithClientListViewModel:Invoice
    {
        public System.Data.DataTable clientsList { get; set; }
        public System.Data.DataTable shipmentsList { get; set; }
    }
}