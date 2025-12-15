using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShippingComp.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "BasePrice is required")]
        public decimal BasePrice { get; set; }
        [Required(ErrorMessage = "Tax is required")]
        public decimal Tax { get; set; }
        public decimal? Total { get; set; }
        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "ShipmentId is required")]
        [ForeignKey("Shipment")]
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
    }
}