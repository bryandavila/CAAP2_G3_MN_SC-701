using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAAP2.Business.Factories;
using CAAP2.Business.Handlers;
using CAAP2.Business.Helpers;
using CAAP2.Models;
using CAAP2.Repository.Repositories;
using CAAP2.Repository.Repositories.Interfaces;
using CAAP2.Business.Helpers;


namespace CAAP2.Services.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<bool> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task ProcessOrdersAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            if (!OrderTimeValidator.IsValidOrderTime())
                return false;

            await _orderRepository.AddAsync(order);
            return true;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _orderRepository.UpdateAsync(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
        }

        public async Task ProcessOrdersAsync()
        {
            var orders = (await _orderRepository.GetAllAsync()).ToList();

            var sorted = orders
                .OrderByDescending(o => o.User.IsPremium)
                .ThenBy(o => o.Priority)
                .ThenBy(o => o.CreatedDate)
                .ToList();

            var deliveryHandler = new DeliveryOrderHandler();
            var pickupHandler = new PickupOrderHandler();

            deliveryHandler.SetNext(pickupHandler);

            await deliveryHandler.HandleAsync(sorted);
        }
    }

    public static class OrderTimeValidator
    {
        public static bool IsValidOrderTime()
        {
            var now = DateTime.Now;
            var day = now.DayOfWeek;
            var hour = now.Hour;

            return (day is >= DayOfWeek.Sunday and <= DayOfWeek.Thursday && hour is >= 10 and < 21)
                || ((day == DayOfWeek.Friday || day == DayOfWeek.Saturday) && hour is >= 11 and < 23);
        }
    }

}
