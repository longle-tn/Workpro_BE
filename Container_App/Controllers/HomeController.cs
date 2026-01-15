using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Container_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("/")]
        public IActionResult Home()
        {
            return View();
        }
    }
}
