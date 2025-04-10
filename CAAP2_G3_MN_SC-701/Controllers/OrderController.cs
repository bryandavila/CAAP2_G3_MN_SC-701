using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CAAP2.Models;
using CAAP2.Services.Services;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Business.Factories;
using Microsoft.EntityFrameworkCore;

namespace CAAP2_G3_MN_SC_701.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly OrdersDbContext _context;
        private readonly OrderFactory _orderFactory;

        public OrderController(IOrderService orderService, OrdersDbContext context)
        {
            _orderService = orderService;
            _context = context;
            _orderFactory = new OrderFactory();
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
            LoadDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            LoadDropDowns(order);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                return View(order);
            }

            var orderType = await _context.OrderTypes.FirstOrDefaultAsync(x => x.Id == order.OrderTypeId);
            var builtOrder = _orderFactory.Create(
                orderType!,
                order.UserID,
                order.OrderDetail ?? "",
                order.TotalAmount,
                order.Priority ?? "Normal"
            );

            var success = await _orderService.CreateOrderAsync(builtOrder);

            if (!success)
            {
                TempData["Error"] = "La orden no se puede crear fuera del horario permitido.";
                return View(order);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            LoadDropDowns(order);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            LoadDropDowns(order);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                return View(order);
            }

            await _orderService.UpdateOrderAsync(order);
            return RedirectToAction(nameof(Index));
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

        private void LoadDropDowns(Order? order = null)
        {
            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "FullName", order?.UserID);
            ViewData["OrderTypeId"] = new SelectList(_context.OrderTypes, "Id", "Name", order?.OrderTypeId);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return PartialView("_OrderDetailsPartial", order);
        }

    }
}