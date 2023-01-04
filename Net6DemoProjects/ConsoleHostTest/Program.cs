// See https://aka.ms/new-console-template for more information
using ConsoleHostTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

//var host = Host.CreateDefaultBuilder().Build();
// initiazlize configuration manager 
ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
configurationBuilder
    .AddJsonFile("appSettings.json")
        .AddEnvironmentVariables()
    .AddEnvironmentVariables("PROCESSOR_");
var configuration = configurationBuilder.Build();
Console.WriteLine(configuration);
