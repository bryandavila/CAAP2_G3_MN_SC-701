using Xunit;
using CAAP2.Models;
using CAAP2.Business.Handlers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CAAP2.Tests.Business
{
    public class ChainHandlerTests
    {
        [Fact]
        public async Task DeliveryHandler_ShouldProcessDeliveryOrder()
        {
            // Arrange
            var deliveryOrder = new Order
            {
                OrderID = 1,
                OrderTypeId = 2, // Delivery
                OrderType = new OrderType { Id = 2, Name = "Delivery" },
                User = new User { UserID = 1, FullName = "Alice", IsPremium = true },
                Priority = "High",
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            var handler = new DeliveryOrderHandler();

            // Act
            await handler.HandleAsync(new List<Order> { deliveryOrder });

            // Assert
            Assert.Equal("Processed", deliveryOrder.Status);
        }

        [Fact]
        public async Task DeliveryHandler_ShouldPassToNextHandler_WhenNotDelivery()
        {
            // Arrange
            var pickupOrder = new Order
            {
                OrderID = 2,
                OrderTypeId = 1, // Pickup
                OrderType = new OrderType { Id = 1, Name = "Pickup" },
                User = new User { UserID = 2, FullName = "Bob", IsPremium = false },
                Priority = "Medium",
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            var pickupHandler = new PickupOrderHandler();
            var deliveryHandler = new DeliveryOrderHandler();
            deliveryHandler.SetNext(pickupHandler); // Encadenar handlers

            // Act
            await deliveryHandler.HandleAsync(new List<Order> { pickupOrder });

            // Assert
            Assert.Equal("Processed", pickupOrder.Status);
        }

        [Fact]
        public async Task PickupHandler_ShouldOnlyProcessPickupOrders()
        {
            // Arrange
            var pickupOrder = new Order
            {
                OrderID = 3,
                OrderTypeId = 1,
                OrderType = new OrderType { Id = 1, Name = "Pickup" },
                User = new User { UserID = 3, FullName = "Carlos", IsPremium = false },
                Priority = "Low",
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            var handler = new PickupOrderHandler();

            // Act
            await handler.HandleAsync(new List<Order> { pickupOrder });

            // Assert
            Assert.Equal("Processed", pickupOrder.Status);
        }
    }
}
