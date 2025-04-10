using System;
using System.Collections.Generic;

namespace CAAP2.Data.MSSQL.OrdersDB;

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

    public virtual OrderType OrderType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
