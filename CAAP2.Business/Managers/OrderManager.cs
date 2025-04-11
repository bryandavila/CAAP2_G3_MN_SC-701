using CAAP2.Models;
using CAAP2.Repository.Repositories.Interfaces;

namespace CAAP2.Business.Managers;

public interface IOrderManager
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
}

public class OrderManager : IOrderManager
{
    private readonly IOrderRepository _orderRepository;
    public OrderManager(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }
}
