using CAAP2.Data.MSSQL.OrdersDB;
using Microsoft.EntityFrameworkCore;
using CAAP2.Repository.Repositories;
using CAAP2.Repository.Repositories.Interfaces;
using CAAP2.Services.Services;
using CAAP2.Business.Factories;
using CAAP2.Business.Managers;
using CAAP2.Services.External;
using Microsoft.AspNetCore.Authentication.Cookies;



var builder = WebApplication.CreateBuilder(args);

// Configuración de DbContext
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersConnection")));

// Agrega servicios de controllers y vistas (para MVC)
builder.Services.AddControllersWithViews();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderManager, OrderManager>();
builder.Services.AddScoped(typeof(IMinimalRepository<>), typeof(MinimalRepository<>));
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<OrderFactory>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// Rutas por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
