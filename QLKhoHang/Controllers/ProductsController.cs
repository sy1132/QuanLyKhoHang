using Microsoft.AspNetCore.Mvc;

namespace QLKhoHang.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();

        }
    }
}
