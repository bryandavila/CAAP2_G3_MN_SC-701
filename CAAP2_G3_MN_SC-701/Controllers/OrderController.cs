using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CAAP2.Models;
using CAAP2.Services.Services;
using CAAP2.Data.MSSQL.OrdersDB;
using Microsoft.EntityFrameworkCore;

namespace CAAP2_G3_MN_SC_701.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly OrdersDbContext _context;

        public OrderController(IOrderService orderService, OrdersDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "FullName");
            ViewData["OrderTypeId"] = new SelectList(_context.OrderTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                await _orderService.CreateOrderAsync(order);
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "FullName", order.UserID);
            ViewData["OrderTypeId"] = new SelectList(_context.OrderTypes, "Id", "Name", order.OrderTypeId);
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "FullName", order.UserID);
            ViewData["OrderTypeId"] = new SelectList(_context.OrderTypes, "Id", "Name", order.OrderTypeId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                await _orderService.UpdateOrderAsync(order);
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "FullName", order.UserID);
            ViewData["OrderTypeId"] = new SelectList(_context.OrderTypes, "Id", "Name", order.OrderTypeId);
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
