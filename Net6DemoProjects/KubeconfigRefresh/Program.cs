using KubeconfigRefresh;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.k8s.json", optional: true, reloadOnChange: true);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<BackgroundconfigChecker>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
