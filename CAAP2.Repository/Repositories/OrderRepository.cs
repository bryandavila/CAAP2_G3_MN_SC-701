using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Models;
using CAAP2.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CAAP2.Repository.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDbContext _context;

        public OrderRepository(OrdersDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderType)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderType)
                .FirstOrDefaultAsync(o => o.OrderID == id);
        }

        public async Task AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }

    public interface IMinimalRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
    }

    public class MinimalRepository<T> : IMinimalRepository<T> where T : class
    {
        private readonly OrdersDbContext _context;
        public MinimalRepository(OrdersDbContext context)
        {
            _context = context;
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

    }
}

