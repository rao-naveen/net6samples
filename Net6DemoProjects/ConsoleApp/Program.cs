// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;

ConfigurationManager configurationManager = new ConfigurationManager();
//configurationManager.SetBasePath(Directory.GetCurrentDirectory());
configurationManager.AddJsonFile("appSettings.json");
var loggingSection = configurationManager.GetSection("Logging");

var log = loggingSection.Get<ConsoleApp.Logging>();
Console.WriteLine(log);
ConsoleApp.LogLevel l1 = new ConsoleApp.LogLevel() { LogCategory = "", logValue = "" };


