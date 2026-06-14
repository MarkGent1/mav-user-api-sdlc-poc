using Mav.UserMgmt.Api.Setup;
using System.Diagnostics.CodeAnalysis;

var app = CreateWebApplication(args);
await app.RunAsync();
return;

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureBuilder(builder);

    var app = builder.Build();

    app.ConfigureRequestPipeline();

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddHttpContextAccessor();

    builder.Services
        .AddHttpClient("DefaultClient")
        .AddHeaderPropagation();

    builder.Services.ConfigureApi(builder.Configuration);
}

public partial class Program { }
