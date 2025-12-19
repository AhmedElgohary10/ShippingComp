using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ShippingComp.ViewModels
{
    public class PaymentsDataTable_With_ClientsListViewModel
    {
        public DataTable Payments { get; set; }
        public DataTable Clients { get; set; }
    }
}