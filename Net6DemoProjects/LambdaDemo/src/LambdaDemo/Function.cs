using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaDemo
{
    public abstract class FunctionBase
    {
        // https://github.com/LambdaSharp/LambdaSharpTool/blob/98d31a1af22aca098b4f2e531fbb652425499bb4/src/LambdaSharp/ALambdaFunction.cs#L70
        protected static IServiceProvider ? ServiceProvider { get; private set; }
        protected static ILoggerFactory ? LoggerFactory { get; private set; }
        protected static IConfiguration ? Configuration { get; private set; }
        protected FunctionBase()
        {
            // initiazlize configuration manager 
            ConfigurationManager configurationManager = new ConfigurationManager();
            configurationManager
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()
                .AddEnvironmentVariables("AWS_");

            IServiceCollection services =
                new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    var loggingConfig = configurationManager.GetSection("Logging");
                    loggingBuilder.AddConfiguration(loggingConfig);
                    loggingBuilder.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                    });
                });
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();
            var configurationBuilder = configurationManager as IConfigurationBuilder;
            Configuration = configurationBuilder.Build();


        }
        /// <summary>
        /// Configures services to be injected into the lambda's IoC container.
        /// </summary>
        /// <param name="services">Collection of services that are injected into the lambda's IoC container.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            
        }
    }
    public class Function : FunctionBase
    {

        public Function()
        {
            
        }
        /// <summary>
        /// A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Casing FunctionHandler(string input, ILambdaContext context)
        {
            return new Casing(input.ToLower(), input.ToUpper());
        }
    }

    public record Casing(string Lower, string Upper);
}