using Microsoft.AspNetCore.Mvc;

namespace Flexifit.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Hahanapin nito ang Views/Users/Index.cshtml
        }
    }
}