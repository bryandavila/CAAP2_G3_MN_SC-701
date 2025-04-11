using System;
using System.Collections.Generic;

namespace CAAP2.Models;

public partial class OrderType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
