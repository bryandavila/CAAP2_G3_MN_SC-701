using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAAP2.Models;
using CAAP2.Models.DTOs;
using CAAP2.Services.Services;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Business.Factories;
using System.Globalization;
using CAAP2.Services.External;
using Microsoft.AspNetCore.Authorization;

namespace CAAP2_G3_MN_SC_701.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly OrdersDbContext _context;
        private readonly OrderFactory _orderFactory;
        private readonly IExchangeRateService _exchangeRateService;

        public OrderController(IOrderService orderService, OrdersDbContext context, IExchangeRateService exchangeRateService)
        {
            _orderService = orderService;
            _context = context;
            _orderFactory = new OrderFactory();
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var exchange = await _exchangeRateService.GetExchangeRateAsync();
            ViewBag.ExchangeRate = exchange;
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
        public async Task<IActionResult> Create()
        {
            await LoadDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var order = new Order();

            order.OrderDetail = form["OrderDetail"];
            order.Priority = form["Priority"];
            order.CreatedDate = DateTime.Now;
            order.Status = "Pending";
            order.UserID = int.TryParse(form["UserID"], out var uid) ? uid : 0;
            order.OrderTypeId = int.TryParse(form["OrderTypeId"], out var otid) ? otid : 0;

            var now = DateTime.Now;
            var day = now.DayOfWeek;
            var hour = now.TimeOfDay;
            TimeSpan start, end;

            if (day == DayOfWeek.Friday || day == DayOfWeek.Saturday)
            {
                start = new TimeSpan(11, 0, 0);
                end = new TimeSpan(23, 0, 0);
            }
            else
            {
                start = new TimeSpan(10, 0, 0);
                end = new TimeSpan(21, 0, 0);
            }

            if (hour < start || hour > end)
            {
                TempData["Error"] = $"No se pueden registrar órdenes en este horario. Hoy puedes hacerlo entre {start:hh\\:mm} y {end:hh\\:mm}.";
                await LoadDropDowns();
                return View(order);
            }

            if (decimal.TryParse(form["TotalAmount"], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                order.TotalAmount = amount;
            }
            else
            {
                ModelState.AddModelError("TotalAmount", "El monto debe ser un número válido con punto decimal (Ej: 25.50)");
            }

            if (order.UserID == 0)
                ModelState.AddModelError("UserID", "Debe seleccionar un usuario.");

            if (order.OrderTypeId == 0)
                ModelState.AddModelError("OrderTypeId", "Debe seleccionar un tipo de orden.");

            if (string.IsNullOrWhiteSpace(order.OrderDetail))
                ModelState.AddModelError("OrderDetail", "Debe escribir un detalle para la orden.");

            if (string.IsNullOrWhiteSpace(order.Priority))
                ModelState.AddModelError("Priority", "Debe seleccionar una prioridad.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                await LoadDropDowns();
                return View(order);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropDowns(Order? order = null)
        {
            try
            {
                var complex = await _orderService.GetAllDataAsync();

                ViewBag.Users = complex.OrderUsers != null
                    ? complex.OrderUsers.Select(u => new User { UserID = u.UserID, FullName = u.FullName }).ToList()
                    : new List<User>();

                ViewBag.OrderTypes = complex.OrderTypes != null
                    ? complex.OrderTypes.Select(t => new OrderType { Id = t.Id, Name = t.Name }).ToList()
                    : new List<OrderType>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cargando dropdowns: " + ex.Message);
                ViewBag.Users = new List<User>();
                ViewBag.OrderTypes = new List<OrderType>();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == "Processed" || order.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "Esta orden no puede ser editada porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropDowns();
            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection form)
        {
            int.TryParse(form["OrderID"], out int orderId);
            var original = await _orderService.GetOrderByIdAsync(orderId);

            if (original == null)
                return NotFound();

            if (original.Status == "Processed" || original.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "Esta orden no puede ser modificada porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            original.OrderDetail = form["OrderDetail"];
            original.Priority = form["Priority"];
            original.UserID = int.TryParse(form["UserID"], out var uid) ? uid : 0;
            original.OrderTypeId = int.TryParse(form["OrderTypeId"], out var otid) ? otid : 0;

            if (decimal.TryParse(form["TotalAmount"], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                original.TotalAmount = amount;
            }
            else
            {
                ModelState.AddModelError("TotalAmount", "El monto debe ser un número válido con punto decimal (Ej: 25.50)");
            }

            if (original.UserID == 0)
                ModelState.AddModelError("UserID", "Debe seleccionar un usuario.");

            if (original.OrderTypeId == 0)
                ModelState.AddModelError("OrderTypeId", "Debe seleccionar un tipo de orden.");

            if (string.IsNullOrWhiteSpace(original.OrderDetail))
                ModelState.AddModelError("OrderDetail", "Debe escribir un detalle para la orden.");

            if (string.IsNullOrWhiteSpace(original.Priority))
                ModelState.AddModelError("Priority", "Debe seleccionar una prioridad.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                await LoadDropDowns();
                return View(original);
            }

            await _orderService.UpdateOrderAsync(original);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == "Processed" || order.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "Esta orden no puede ser eliminada porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(IFormCollection form)
        {
            if (!int.TryParse(form["OrderID"], out int orderId))
            {
                TempData["Error"] = "No se pudo identificar la orden a eliminar.";
                return RedirectToAction(nameof(Index));
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound();

            if (order.Status == "Processed" || order.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "No se puede eliminar esta orden porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            await _orderService.DeleteOrderAsync(orderId);
            TempData["Success"] = "Orden eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return PartialView("_OrderDetailsPartial", order);
        }

        [HttpGet]
        public async Task<IActionResult> ProcessOrders()
        {
            await _orderService.ProcessOrdersAsync();
            TempData["ProcessResult"] = "Órdenes procesadas correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteOrder([FromBody] ExecuteOrderRequest request)
        {
            var order = await _orderService.GetOrderByIdAsync(request.Id);

            if (order == null)
                return Json(new { success = false, message = "Orden no encontrada" });

            if (order.Status == "Processed")
                return Json(new { success = false, message = "La orden ya fue procesada" });

            await Task.Delay(10000);

            order.Status = "Processed";
            await _orderService.UpdateOrderAsync(order);

            return Json(new { success = true });
        }
    }
}