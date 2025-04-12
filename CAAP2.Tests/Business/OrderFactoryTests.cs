using Xunit;
using CAAP2.Models;
using CAAP2.Business.Factories;
using System.Collections.Generic;

namespace CAAP2.Tests.Business
{
    public class OrderFactoryTests
    {
        private readonly OrderFactory _factory;

        public OrderFactoryTests()
        {
            _factory = new OrderFactory();
        }

        [Fact]
        public void Create_WithDeliveryType_ReturnsDeliveryOrder()
        {
            // Arrange
            var orderType = new OrderType
            {
                Id = 2,
                Name = "Delivery"
            };

            var userId = 1;
            var expectedDetail = "2x Hamburguesas";
            var expectedAmount = 15.00m;
            var expectedPriority = "Alta";

            // Act
            var order = _factory.Create(orderType, userId, expectedDetail, expectedAmount, expectedPriority);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(2, order.OrderTypeId); // Delivery
            Assert.Equal(userId, order.UserID);
            Assert.Equal(expectedDetail, order.OrderDetail);
            Assert.Equal(expectedAmount, order.TotalAmount);
            Assert.Equal(expectedPriority, order.Priority);
            Assert.Equal("Pending", order.Status);
        }

        [Fact]
        public void Create_WithPickupType_ReturnsPickupOrder()
        {
            // Arrange
            var orderType = new OrderType
            {
                Id = 1,
                Name = "Pickup"
            };

            var userId = 2;
            var detail = "1x Pizza";
            var amount = 10.50m;
            var priority = "Media";

            // Act
            var order = _factory.Create(orderType, userId, detail, amount, priority);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(1, order.OrderTypeId); // Pickup
            Assert.Equal(userId, order.UserID);
            Assert.Equal("Pending", order.Status);
        }
    }
}
