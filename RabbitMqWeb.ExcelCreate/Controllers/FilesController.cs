using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMqWeb.ExcelCreate.Hubs;
using RabbitMqWeb.ExcelCreate.Models;

namespace RabbitMqWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ILogger _logger;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// worker servicemiz tarafından excel dosyası oluşturulduğunda çağrılır
        /// </summary>    
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            try
            {
                if (file is not { Length: > 0 }) return BadRequest();

                var userFile = await _context.UserFiles.FirstAsync(x => x.Id == fileId);

                var filePath = userFile.FileName + Path.GetExtension(file.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

                using FileStream stream = new(path, FileMode.Create);

                await file.CopyToAsync(stream);

                userFile.CreatedDate = DateTime.Now;
                userFile.FilePath = filePath;
                userFile.FileStatus = FileStatus.Completed;

                await _context.SaveChangesAsync();
                              

                //SignalR ile sisteme authontication olmuş kullanıcıya dosya oluşturulduğu bilgisini gönderiyoruz
                await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

                return Ok();

            }
            catch (Exception ex)
            {
                // Hata meydana gelirse loglama yapın
                _logger.LogError(ex, "Upload method encountered an error: {ErrorMessage}", ex.Message);
                return StatusCode(500, "An error occurred while processing the request.");
            }

        }
    }
}
