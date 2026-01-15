using Container_App.Core.Interface.Users;
using Container_App.Core.Model.Users;
using Microsoft.AspNetCore.Mvc;

namespace Container_App.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost]
        [Route("api/insert-user")]
        public async Task<IActionResult> Insert([FromBody] UserProfile u)
        {
            int result = await _userServices.Insert(u);
            if (result == 0)
            {
                return BadRequest();
            }
            return Ok(result);
        }
    }
}
