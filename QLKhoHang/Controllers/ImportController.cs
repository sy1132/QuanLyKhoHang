using Microsoft.AspNetCore.Mvc;

namespace QLKhoHang.Controllers
{
    public class ImportController : Controller
    {
        public IActionResult Index()
        {
            return View(); 



        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
