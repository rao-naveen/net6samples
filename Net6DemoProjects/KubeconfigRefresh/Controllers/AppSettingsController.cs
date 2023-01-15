using Microsoft.AspNetCore.Mvc;

namespace KubeconfigRefresh.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class AppConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AppConfigController(IConfiguration config)
        {
            _configuration = config;
        }

        [HttpGet]
        public IActionResult GetConfigurationSettings()
        {
            return Ok(_configuration.GetSection("appConfiguration").GetChildren());
        }
    }
}
