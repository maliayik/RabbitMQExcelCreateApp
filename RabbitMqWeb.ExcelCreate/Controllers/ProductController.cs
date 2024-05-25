using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMqWeb.ExcelCreate.Controllers
{
    public class ProductController : Controller
    {
        //sadece login olan kullanıcılar bu sayfayı görebilir
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
