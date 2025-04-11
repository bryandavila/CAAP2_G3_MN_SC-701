using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAAP2.Models;

namespace CAAP2.Business.Factories
{
    public class OrderFactory
    {
        public Order Create(OrderType orderType, int userId, string detail, decimal amount, string priority)
        {
            var order = new Order
            {
                OrderTypeId = orderType.Id,
                OrderDetail = detail,
                TotalAmount = amount,
                Priority = priority,
                UserID = userId,
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            // Lógica específica si se requiere según tipo
            if (orderType.Name == "Delivery")
            {
                // ajustes para delivery
            }
            else if (orderType.Name == "Pickup")
            {
                // ajustes para pickup
            }

            return order;
        }
    }
}
