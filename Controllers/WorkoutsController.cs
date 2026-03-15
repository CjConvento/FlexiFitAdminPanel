using Microsoft.AspNetCore.Mvc;

namespace Flexifit.Controllers
{
    public class WorkoutsController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Hahanapin nito ang Views/Workouts/Index.cshtml
        }
    }
}