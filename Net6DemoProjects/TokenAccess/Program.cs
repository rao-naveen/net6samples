// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IdentityModel.Client;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddAccessTokenManagement(options =>
        {
            options.Client.Clients.Add("identityserver", new ClientCredentialsTokenRequest
            {
                Address = "https://demo.identityserver.io/connect/token",
                ClientId = "m2m.short",
                ClientSecret = "secret",
                Scope = "api" // optional
            });
        });
        services.AddClientAccessTokenHttpClient("client", configureClient: client =>
        {
            client.BaseAddress = new Uri("www.google.com");
        });
    });

