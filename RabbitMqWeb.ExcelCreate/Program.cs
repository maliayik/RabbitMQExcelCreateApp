using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMqWeb.ExcelCreate.Models;
using RabbitMqWeb.ExcelCreate.Services;
using System.Configuration;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(sp=> new ConnectionFactory() { Uri=new Uri(builder.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
builder.Services.AddSingleton <RabbitMQClientService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddIdentity<IdentityUser,IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

}).AddEntityFrameworkStores<AppDbContext>();



var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var appDbContext = services.GetRequiredService<AppDbContext>();
        appDbContext.Database.Migrate();//uygulama ayaða kalktýðýnda migration olusturur.

        if (!appDbContext.Users.Any())
        {
            userManager.CreateAsync(new IdentityUser() { UserName = "admin", Email = "test@hotmail.com" }, "Password12*").Wait();//asenkronu senkrona cevirmek icin wait kullandýk

            userManager.CreateAsync(new IdentityUser() { UserName = "admin2", Email = "test2@hotmail.com" }, "Password12*").Wait();

        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "migration sýrasýnda hata meydana geldi.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); //üyelik sistemi oldugu için ekledik.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
