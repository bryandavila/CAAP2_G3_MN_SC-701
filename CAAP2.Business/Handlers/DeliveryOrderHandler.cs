using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CAAP2.Models;

namespace CAAP2.Business.Handlers;

public class DeliveryOrderHandler : OrderHandlerBase
{
    public override async Task HandleAsync(List<Order> orders)
    {
        var deliveryOrders = orders
            .Where(o => o.OrderType.Name == "Delivery")
            .Take(5)
            .ToList();

        foreach (var order in deliveryOrders)
        {
            await ProcessOrderAsync(order);
        }

        // Remove processed
        orders.RemoveAll(o => deliveryOrders.Contains(o));

        // Pass remaining to next handler
        if (_nextHandler != null)
            await _nextHandler.HandleAsync(orders);
    }

    private async Task ProcessOrderAsync(Order order)
    {
        Debug.WriteLine($"Procesando Delivery: Orden {order.OrderID}");
        await Task.Delay(10000); // Simula 10 segundos de procesamiento
        order.Status = "Processed";
    }
}

