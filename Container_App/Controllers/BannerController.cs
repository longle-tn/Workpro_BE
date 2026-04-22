using Container_App.Core.Interface.Banners;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Container_App.Controllers
{  
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;
        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }
    }
}
