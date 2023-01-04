// See https://aka.ms/new-console-template for more information
using MELConsoleLogIntegraton;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Logging.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;


//StringWriter stringWriter = new StringWriter();
//var cout = Console.Out;
//Console.SetOut(stringWriter);

ConfigurationManager configurationManager = new ConfigurationManager();
//configurationManager.SetBasePath(Directory.GetCurrentDirectory());
configurationManager.AddJsonFile("appSettings.json");

//using var loggerNullFactory = LoggerFactory.Create(builder =>
//{
//    builder.Services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
//   //{
//    //    options.SingleLine = true;
//    //});
//});
//var log = loggerNullFactory.CreateLogger("demo");
//log.LogInformation("abc");




using var loggerFactory = LoggerFactory.Create(builder =>
{
    var loggingConfig = configurationManager.GetSection("Logging");
    builder.AddConfiguration(loggingConfig);
    builder.AddConsole()
           .AddCustomFormatter(options =>
           {
               options.IncludeScopes = true;
               options.UseUtcTimestamp = true;
               options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
           });
    //builder.AddSimpleConsole(options =>
    //{
    //    options.SingleLine = true;
    //});
});
loggerFactory.AddMemory();


//Console.WriteLine(configurationManager["Logging:LogLevel:Default"]);
//Console.WriteLine(configurationManager.GetSection("Logging:LogLevel").GetChildren().Count());
var builder = configurationManager as IConfigurationBuilder;
var root = builder.Build();
//Console.WriteLine(root["Logging:LogLevel:Default"]);


var serviceLogger = loggerFactory.CreateLogger("Demo.Service");
serviceLogger.LogInformation("This is service log");

TenantInformation tenantInformation = new TenantInformation() { Id= "64F180747B8B4B03B893DA5731236172", Name = "Apollo" };
serviceLogger.LogTenantInformation(tenantInformation);


//serviceLogger.LogInformation(Constants.ServiceEventId1, "service message");
//serviceLogger.LogInformation(Constants.ServiceEventId2, "service message", 20);

//serviceLogger.LogInformation(Constants.UsageEventId1, "usage info {count}", 10);
//serviceLogger.LogInformation(Constants.UsageEventId2, "usage info {count}", 20);


var customerLogger = loggerFactory.CreateLogger("Demo.Customer");

customerLogger.LogInformation("This is customer log");

//var logList = MemoryLogger.LogList;


//Console.SetOut(cout);
//Console.WriteLine(stringWriter.ToString());