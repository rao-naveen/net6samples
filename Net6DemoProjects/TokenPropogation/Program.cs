var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHeaderPropagation(o =>
 {
     // propogating the HttpRequest Headers
     o.Headers.Add("Authorization");
 });
builder.Services.AddHttpClient("DemoApp").AddHeaderPropagation();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.UseHeaderPropagation();
app.MapControllers();

app.Run();


