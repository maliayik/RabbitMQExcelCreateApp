using FileCreateWorkerService;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using FileCreateWorkerService.Models;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
      
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<AdventureWorks2019Context>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });

        // RabbitMQ baðlantý ayarlarýný yapýlandýrma dosyasýndan çekiyoruz
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");

        // RabbitMQ baðlantý nesnesini oluþturup DI konteynerine ekliyoruz
        services.AddSingleton(sp => new ConnectionFactory()
        {
            Uri = new Uri(rabbitMqConnectionString),
            DispatchConsumersAsync = true
        });

        services.AddSingleton<RabbitMQClientService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
