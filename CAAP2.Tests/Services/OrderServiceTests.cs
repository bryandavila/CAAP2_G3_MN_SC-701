using Xunit;
using Moq;
using CAAP2.Models;
using CAAP2.Services.Services;
using CAAP2.Repository.Repositories.Interfaces;
using CAAP2.Tests.Fakes; // ✅ Importa el handler simulado
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CAAP2.Repository.Repositories;

namespace CAAP2.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IMinimalRepository<OrderType>> _orderTypeRepoMock;
        private readonly Mock<IMinimalRepository<User>> _userRepoMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderTypeRepoMock = new Mock<IMinimalRepository<OrderType>>();
            _userRepoMock = new Mock<IMinimalRepository<User>>();

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _orderTypeRepoMock.Object,
                _userRepoMock.Object,
                new TestOrderHandler() // ✅ Handler simulado
            );
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnCorrectOrder()
        {
            var expectedOrder = new Order { OrderID = 1, OrderDetail = "1x Taco" };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(expectedOrder);

            var result = await _orderService.GetOrderByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("1x Taco", result.OrderDetail);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCallRepository()
        {
            var newOrder = new Order { OrderID = 99, OrderDetail = "2x Sushi", TotalAmount = 25.5m };

            _orderRepoMock.Setup(repo => repo.AddAsync(newOrder)).Returns(Task.CompletedTask);

            var result = await _orderService.CreateOrderAsync(newOrder);

            Assert.True(result);
            _orderRepoMock.Verify(repo => repo.AddAsync(newOrder), Times.Once);
        }

        [Fact]
        public async Task ProcessOrdersAsync_ShouldUpdateEachPendingOrder()
        {
            var orders = new List<Order>
            {
                new Order
                {
                    OrderID = 1,
                    Status = "Pending",
                    OrderTypeId = 2,
                    OrderType = new OrderType { Id = 2, Name = "Delivery" },
                    User = new User { UserID = 1, FullName = "Alice", IsPremium = true },
                    Priority = "High",
                    CreatedDate = DateTime.Now
                },
                new Order
                {
                    OrderID = 2,
                    Status = "Pending",
                    OrderTypeId = 1,
                    OrderType = new OrderType { Id = 1, Name = "Pickup" },
                    User = new User { UserID = 2, FullName = "Bob", IsPremium = false },
                    Priority = "Medium",
                    CreatedDate = DateTime.Now
                }
            };

            _orderRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(orders);
            _orderRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            await _orderService.ProcessOrdersAsync();

            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Exactly(2));
        }
    }
}
