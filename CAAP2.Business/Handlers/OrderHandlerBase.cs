using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAAP2.Business.Handlers;

using CAAP2.Models;

public abstract class OrderHandlerBase
{
    protected OrderHandlerBase? _nextHandler;

    public void SetNext(OrderHandlerBase nextHandler)
    {
        _nextHandler = nextHandler;
    }

    public abstract Task HandleAsync(List<Order> orders);
}
