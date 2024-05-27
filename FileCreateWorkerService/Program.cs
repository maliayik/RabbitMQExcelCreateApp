using FileCreateWorkerService;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
      
        IConfiguration configuration = hostContext.Configuration;

        // RabbitMQ ba�lant� ayarlar�n� yap�land�rma dosyas�ndan �ekiyoruz
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");

        // RabbitMQ ba�lant� nesnesini olu�turup DI konteynerine ekliyoruz
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
