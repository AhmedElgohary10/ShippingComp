using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShippingComp.Models
{
    public class Shipment
    {
        public int id { get; set; }
        [Required(ErrorMessage = "TrackingNumber is required")]
        public string TrackingNumber { get; set; }
        [Required(ErrorMessage = "ShipmentDate is required")]
        public DateTime ShipmentDate { get; set; }
        [Required(ErrorMessage = "Weight is required")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Client/ Shipment Owner Id is required")]
        [ForeignKey("shipmentOwner")]
        public int ClientId { get; set; }

        Client shipmentOwner;
    }
}