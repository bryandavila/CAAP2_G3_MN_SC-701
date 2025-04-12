using CAAP2.Models;
using CAAP2.Business.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAAP2.Tests.Fakes
{
    public class TestOrderHandler : OrderHandlerBase
    {
        public override async Task HandleAsync(List<Order> orders)
        {
            foreach (var order in orders)
            {
                order.Status = "Processed"; // simular procesamiento
            }

            await Task.CompletedTask;
        }
    }
}
