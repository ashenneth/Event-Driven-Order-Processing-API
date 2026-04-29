using Microsoft.EntityFrameworkCore;
using OrderApi.Middleware;
using OrderApi.Persistence;
using OrderApi.Repositories;
using OrderApi.Services;
using Microsoft.Extensions.Options;
using OrderApi.Messaging;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Logs;
using Shared.Observability;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
var aiConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

// Enable Azure Monitor OpenTelemetry
builder.Services.AddOpenTelemetry().UseAzureMonitor(o =>
{
    if (!string.IsNullOrWhiteSpace(aiConn))
        o.ConnectionString = aiConn;

});

// Ensure service name is set (shows nicely in App Insights)
builder.Services.ConfigureOpenTelemetryTracerProvider((sp, tp) =>
{
    tp.AddSource(Telemetry.ActivitySourceName)
      .ConfigureResource(rb => rb.AddService("OrderApi"));
});

// Enable log scopes ? scope values become custom properties in App Insights logs :contentReference[oaicite:2]{index=2}
builder.Services.Configure<OpenTelemetryLoggerOptions>(o => o.IncludeScopes = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(cs);
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection(ServiceBusOptions.SectionName));

builder.Services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

// Middleware DI
builder.Services.AddTransient<CorrelationIdMiddleware>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
