using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShippingComp.Models
{
    public class Payment
    {
        public int InvoiceId { get; set; }
        public int AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}