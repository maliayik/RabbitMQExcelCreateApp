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

        // RabbitMQ bağlantı ayarlarını yapılandırma dosyasından çekiyoruz
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");

        // RabbitMQ bağlantı nesnesini oluşturup DI konteynerine ekliyoruz
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
