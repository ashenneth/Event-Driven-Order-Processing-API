using Microsoft.EntityFrameworkCore;
using OrderProcessor.Worker.Messaging;
using OrderProcessor.Worker.Persistence;
using OrderProcessor.Worker.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Logs;
using Shared.Observability;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;


var builder = Host.CreateApplicationBuilder(args);

var aiConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

builder.Services.AddOpenTelemetry().UseAzureMonitor(o =>
{
    if (!string.IsNullOrWhiteSpace(aiConn))
        o.ConnectionString = aiConn;
});

builder.Services.ConfigureOpenTelemetryTracerProvider((sp, tp) =>
{
    tp.AddSource(Telemetry.ActivitySourceName)
      .ConfigureResource(rb => rb.AddService("OrderProcessor.Worker"));
});

builder.Services.Configure<OpenTelemetryLoggerOptions>(o => o.IncludeScopes = true);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(cs);
});

builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection(ServiceBusOptions.SectionName));

builder.Services.AddScoped<IOrderProcessorService, OrderProcessorService>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();
host.Run();
