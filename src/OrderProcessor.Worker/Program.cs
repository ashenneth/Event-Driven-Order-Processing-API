using Microsoft.EntityFrameworkCore;
using OrderProcessor.Worker.Messaging;
using OrderProcessor.Worker.Persistence;
using OrderProcessor.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

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
