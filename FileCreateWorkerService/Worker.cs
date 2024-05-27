using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Data;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        //appdbcontextimizde scope olarak tan�ml� olan AdventureWorks2019Context db mize ba�nabilmek i�in service provider tan�ml�yoruz
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider, IModel channel)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
            _channel = channel;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);//her bir subscribera bir bir g�nderim yap�lacak



            return base.StartAsync(cancellationToken);
        }



        protected override  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms= new MemoryStream();

            //memoryde excelimizi olusturuyoruz.
            var wb= new XLWorkbook();
            var ds=new DataSet();
            ds.Tables.Add(GetTable("Products"));

            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();

            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()),"file", Guid.NewGuid().ToString()+".xlsx");

            var baseUrl = "https://localhost:44328/api/files";

            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?file�d={createExcelMessage.FileId}", multipartFormDataContent);

                if(response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File ( Id : {createExcelMessage.FileId}) was created by successful");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }

        }

        private DataTable GetTable(string tableName)
        {
            List<Product> products;

            //dbye b�uradan ba�lan�yoruz
            using(var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
                products = context.Products.ToList();
            }   

            //memoryde istedi�imiz verilerle bir tablo olusturuyoruz.
            DataTable table = new DataTable(tableName);

            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(p =>
            {
                table.Rows.Add(p.ProductId, p.Name, p.ProductNumber, p.Color);
            });

            return table;
        }

    }
}
