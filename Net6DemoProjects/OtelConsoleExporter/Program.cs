// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OtelConsoleExporter;
using System.Diagnostics;

ActivitySource MyActivitySource = new("OtelConsoleExporter.Demo");
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
           .AddSource("OtelConsoleExporter.Demo")
           //.AddConsoleExporter()
           .Build();


using var loggerFactory = LoggerFactory.Create(builder =>
{
   
    builder.AddOpenTelemetry(options =>
    {
        //options.AddConsoleExporter();
        options.AddMyExporter();
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        
    });
});
var logger = loggerFactory.CreateLogger("OtelConsoleExporter");
//using (var activity = MyActivitySource.StartActivity("SayHello"))
//{
    using (logger.BeginScope(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("key1", "value1") }))
    {
        logger.LogInformation(new EventId(1000), "Hello from {name} {price}.", "tomato", 2.99);
        logger.LogWarning(new EventId(2000),"warning message");
        logger.LogError(new EventId(5000),"Error Message", new Exception("some exception"));
}
        
//}
    


