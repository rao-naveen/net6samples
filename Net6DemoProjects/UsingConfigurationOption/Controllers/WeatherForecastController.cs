using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UsingConfigurationOption.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration configuration;
        private readonly IOptionsMonitor<ApplicationInfo> appInfo;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IConfiguration configuration,
            IOptionsMonitor<ApplicationInfo> appInfo)
        {
            _logger = logger;
            this.configuration = configuration;
            this.appInfo = appInfo;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)] + $"Application Name {appInfo.CurrentValue}"
            })
            .ToArray();
        }

        // reloading the configuration
        // https://mbarkt3sto.hashnode.dev/understanding-ioptionsmonitort-in-aspnet-core
        [HttpPost]
        [Route("update")]
        public IActionResult Update()
        {
            configuration["applicationInfo:id"] = "xyz124 " + DateTime.Now.ToString();
            if (configuration is IConfigurationRoot configurationRoot)
            {
                configurationRoot.Reload();
            }
            
            
            return Ok();
        }

    }
}