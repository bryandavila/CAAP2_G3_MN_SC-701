using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAP2.Models
{
    public partial class Order
    {
        public int OrderID { get; set; }

        public int UserID { get; set; }

        public string? OrderDetail { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? Priority { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Status { get; set; }

        public int OrderTypeId { get; set; }

        [NotMapped]
        public virtual OrderType? OrderType { get; set; }

        [NotMapped]
        public virtual User? User { get; set; }
    }
}
