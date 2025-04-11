using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAAP2.Models;
using CAAP2.Services.Services;
using CAAP2.Data.MSSQL.OrdersDB;
using CAAP2.Business.Factories;

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
        public async Task<IActionResult> Create()
        {
            await LoadDropDowns();
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
            if (orderType == null)
            {
                ModelState.AddModelError("OrderTypeId", "Tipo de orden no válido.");
                LoadDropDowns(order);
                return View(order);
            }

            var builtOrder = _orderFactory.Create(
                orderType,
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

        private async Task LoadDropDowns(Order? order = null)
        {
            try
            {
                //var users = _context.Users.ToList();
                //var orderTypes = _context.OrderTypes.ToList();

                var complex = await _orderService.GetAllDataAsync();

                Console.WriteLine($"🔍 Usuarios encontrados: {complex.OrderUsers.Count()}");
                Console.WriteLine($"🔍 Tipos de orden encontrados: {complex.OrderTypes.Count()}");

                ViewBag.Users = complex.OrderUsers; //new SelectList(, "UserID", "FullName", order?.UserID);
                ViewBag.OrderTypes = complex.OrderTypes; // new SelectList(complex.OrderTypes, "Id", "Name", order?.OrderTypeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Error cargando dropdowns: " + ex.Message);
                ViewBag.Users = new SelectList(Enumerable.Empty<User>(), "UserID", "FullName");
                ViewBag.OrderTypes = new SelectList(Enumerable.Empty<OrderType>(), "Id", "Name");
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

            LoadDropDowns(order);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                LoadDropDowns(order);
                return View(order);
            }

            var original = await _orderService.GetOrderByIdAsync(order.OrderID);
            if (original == null)
                return NotFound();

            if (original.Status == "Processed" || original.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "Esta orden no puede ser modificada porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
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

            if (order.Status == "Processed" || order.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "Esta orden no puede ser eliminada porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == "Processed" || order.CreatedDate?.AddMinutes(1) < DateTime.Now)
            {
                TempData["Error"] = "No se puede eliminar esta orden porque ya fue procesada o ha pasado más de 1 minuto.";
                return RedirectToAction(nameof(Index));
            }

            await _orderService.DeleteOrderAsync(id);
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
