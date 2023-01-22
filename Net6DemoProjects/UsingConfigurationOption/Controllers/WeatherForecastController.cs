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
        /// <summary>
        /// Count = 11
        //{[LogginCount = 11
        //{[Logging:LogLevel:Default, Information]}
        //{[Logging:LogLevel:Microsoft.AspNetCore, Warning]}
        //{[AllowedHosts, *]}
        //{[applicationInfo:name, ConfigurationDemo]}
        //{[applicationInfo:id, 345343434]}
        //{[remoteNodes:0:id, uniqueid1]}
        //{[remoteNodes:0:name, machinea]}
        //{[remoteNodes:1:id, uniqueid2]}
        //{[remoteNodes:1:name, machinebb]}
        //{[remoteNodes:2:id, uniqueid3]}
        //{[remoteNodes:2:name, machinebc]}
    /// </summary>
    /// <returns></returns>
[HttpGet]
        [Route("config")]
        public IActionResult config()
        {
            if (configuration is IConfigurationRoot configurationRoot)
            {
                return Ok(configurationRoot.GetDebugView());
            }
            return Ok("");
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