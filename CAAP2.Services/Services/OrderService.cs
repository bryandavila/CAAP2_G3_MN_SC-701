using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAAP2.Business.Factories;
using CAAP2.Business.Handlers;
using CAAP2.Models;
using CAAP2.Repository.Repositories;
using CAAP2.Repository.Repositories.Interfaces;

namespace CAAP2.Services.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<OrderComplex> GetAllDataAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<bool> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task ProcessOrdersAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMinimalRepository<OrderType> _minimalOrderType;
        private readonly IMinimalRepository<User> _minimalUser;

        public OrderService(IOrderRepository orderRepository, IMinimalRepository<OrderType> minimalOrderType, IMinimalRepository<User> minimalUser)
        {
            _orderRepository = orderRepository;
            _minimalOrderType = minimalOrderType;
            _minimalUser = minimalUser;
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
            var allOrders = (await _orderRepository.GetAllAsync()).ToList();

            // Solo procesamos las órdenes pendientes
            var pendingOrders = allOrders
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.User?.IsPremium ?? false)
                .ThenBy(o => o.Priority)
                .ThenBy(o => o.CreatedDate)
                .ToList();

            if (!pendingOrders.Any())
                return;

            // HANDLERS para procesamiento
            var deliveryHandler = new DeliveryOrderHandler();
            var pickupHandler = new PickupOrderHandler();
            deliveryHandler.SetNext(pickupHandler);

            await deliveryHandler.HandleAsync(pendingOrders);

            // ACTUALIZAR estado a "Processed"
            foreach (var order in pendingOrders)
            {
                order.Status = "Processed";
                await _orderRepository.UpdateAsync(order);
            }
        }


        public async Task<OrderComplex> GetAllDataAsync()
        {
            return new OrderComplex
            {
                Orders = await _orderRepository.GetAllAsync(),
                OrderTypes = _minimalOrderType.GetAll(),
                OrderUsers = _minimalUser.GetAll()
            };
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
                || ((day == DayOfWeek.Friday || day == DayOfWeek.Saturday) && hour is >= 11 && hour < 23);
        }
    }
}
