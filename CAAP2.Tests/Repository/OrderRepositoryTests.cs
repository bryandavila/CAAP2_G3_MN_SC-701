using Xunit;
using CAAP2.Models;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CAAP2.Tests.Repository
{
    public class OrderRepositoryTests
    {
        private OrdersDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // ✅ nombre único por test
                .Options;

            return new OrdersDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOrder()
        {
            // Arrange
            using var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, FullName = "Test User", Email = "test@example.com" };
            var type = new OrderType { Id = 1, Name = "Pickup" };
            var order = new Order
            {
                OrderID = 1,
                OrderDetail = "Test Order",
                User = user,
                OrderType = type,
                Priority = "High",
                Status = "Pending",
                TotalAmount = 10.00m
            };

            context.Users.Add(user);
            context.OrderTypes.Add(type);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repo = new OrderRepository(context);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Order", result.OrderDetail);
            Assert.Equal("Test User", result.User.FullName);
            Assert.Equal("Pickup", result.OrderType.Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOrdersWithUserAndType()
        {
            // Arrange
            using var context = GetInMemoryDbContext();

            var user = new User { UserID = 2, FullName = "User 2", Email = "user2@example.com" };
            var orderType = new OrderType { Id = 2, Name = "Delivery" };

            var orders = new List<Order>
            {
                new Order
                {
                    OrderID = 2,
                    OrderDetail = "Test 1",
                    User = user,
                    OrderType = orderType,
                    Priority = "Medium",
                    Status = "Pending",
                    TotalAmount = 20.00m
                },
                new Order
                {
                    OrderID = 3,
                    OrderDetail = "Test 2",
                    User = user,
                    OrderType = orderType,
                    Priority = "Low",
                    Status = "Pending",
                    TotalAmount = 15.00m
                }
            };

            context.Users.Add(user);
            context.OrderTypes.Add(orderType);
            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            var repo = new OrderRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var orderList = result.ToList();
            Assert.Equal(2, orderList.Count);
            Assert.All(orderList, o =>
            {
                Assert.NotNull(o.User);
                Assert.NotNull(o.OrderType);
            });
        }
    }
}
