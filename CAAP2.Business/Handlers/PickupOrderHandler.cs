using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CAAP2.Models;

namespace CAAP2.Business.Handlers;

public class PickupOrderHandler : OrderHandlerBase
{
    public override async Task HandleAsync(List<Order> orders)
    {
        var pickupOrder = orders
            .Where(o => o.OrderType.Name == "Pickup")
            .Take(1)
            .ToList();

        foreach (var order in pickupOrder)
        {
            await ProcessOrderAsync(order);
        }

        // Remove processed
        orders.RemoveAll(o => pickupOrder.Contains(o));

        // Pass remaining
        if (_nextHandler != null)
            await _nextHandler.HandleAsync(orders);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        Debug.WriteLine($"Procesando Pickup: Orden {order.OrderID}");
        await Task.Delay(10000);
        order.Status = "Processed";
    }
}
