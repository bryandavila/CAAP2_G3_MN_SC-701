using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAAP2.Models
{
    public class OrderComplex
    {
        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<OrderType> OrderTypes { get; set; }
        public IEnumerable<User> OrderUsers { get; set; }
    }
}
