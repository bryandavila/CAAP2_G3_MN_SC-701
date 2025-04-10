using System;
using System.Collections.Generic;

namespace CAAP2.Data.MSSQL.OrdersDB;

public partial class User
{
    public int UserID { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? IsPremium { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsEndUser { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
