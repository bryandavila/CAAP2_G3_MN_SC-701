using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAAP2.Models;
using CAAP2.Services.Services;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Business.Factories;
using System.Globalization;
using CAAP2.Services.External;

namespace CAAP2_G3_MN_SC_701.Controllers
{
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
                    ? complex.OrderUsers.Select(u => new User
                    {
                        UserID = u.UserID,
                        FullName = u.FullName
                    }).ToList()
                    : new List<User>();

                ViewBag.OrderTypes = complex.OrderTypes != null
                    ? complex.OrderTypes.Select(t => new OrderType
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()
                    : new List<OrderType>();

                Console.WriteLine("Usuarios disponibles para dropdown:");
                foreach (var u in ViewBag.Users as List<User>)
                {
                    Console.WriteLine($"🔹 UserID: {u.UserID}, Nombre: {u.FullName}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cargando dropdowns: " + ex.Message);
                ViewBag.Users = new List<User>();
                ViewBag.OrderTypes = new List<OrderType>();
            }
        }


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

    }
}
