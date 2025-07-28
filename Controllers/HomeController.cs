using Microsoft.AspNetCore.Mvc;

namespace Container_App.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Home()
        {
            return View();
        }
    }
}
