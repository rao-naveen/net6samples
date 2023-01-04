using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authorization.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
    .AddOAuth2Introspection(options =>
    {
        options.Authority = "http://localhost/token";
        options.IntrospectionEndpoint = "http://localhost/token";
        options.ClientId = "client_id_for_introspection_endpoint";
        options.ClientSecret = "client_secret_for_introspection_endpoint";
        
        options.Events.OnTokenValidated = e =>
        {
            var user = e.Principal;

            return Task.CompletedTask;
        };

        options.Events.OnUpdateClientAssertion = e =>
        {
            var user = e.Principal;

            return Task.CompletedTask;
        };

        options.Events.OnSendingRequest = e =>
        {
            var user = e.TokenIntrospectionRequest;

            return Task.CompletedTask;
        };
    });

builder.Services.AddHttpClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName)
    .AddHttpMessageHandler(options =>
    {
        return new DemoHandler();
    });
builder.Services.AddAuthorization(configureOptions =>
{
    configureOptions.AddPolicy("globalpolicy", builder =>
    {
        //builder.Requirements.Add(new AssertionRequirement(ctx =>
        //{
        //    var user = ctx.User;
            
        //    return true;
        //}));
        builder.Requirements.Add(new ClaimsAuthorizationRequirement("username", new[] { "chandler bing" }));
    });
    configureOptions.AddPolicy("policy2", builder =>
    {
        builder.Requirements.Add(new ClaimsAuthorizationRequirement("scope", new[] { "email" }));
        //builder.Requirements.Add(new AssertionRequirement(ctx =>
        //{
        //    var user = ctx.User;
        //    return true;
        //}));
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization("globalpolicy");

app.Run();
