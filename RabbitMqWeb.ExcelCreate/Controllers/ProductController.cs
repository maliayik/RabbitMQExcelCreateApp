using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMqWeb.ExcelCreate.Models;

namespace RabbitMqWeb.ExcelCreate.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //sadece login olan kullanıcılar bu sayfayı görebilir
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        //burada sadece productu excele çeviricegimiz için her hangi bir parametre almıyoruz.
        //Ama normalde büyük raporlarda birden fazla join işlemi olduğu için parametre kullanmamız gerekir.
        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
          

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await _context.UserFiles.AddAsync(userFile);

            await _context.SaveChangesAsync();

            //rabbitmq'ya mesaj gönderiyoruz.
            TempData["StartCreatingExcel"] = true; //bir requestten başka bir requeste data taşımak için tempdata kullanıyoruz.

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);         

            return View(await _context.UserFiles.Where(x=> x.UserId==user.Id).ToListAsync());
        }
    }
}
