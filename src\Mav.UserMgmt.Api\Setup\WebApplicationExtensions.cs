namespace Mav.UserMgmt.Api.Setup;

public static class WebApplicationExtensions
{
    public static void ConfigureRequestPipeline(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var configuration = app.Services.GetRequiredService<IConfiguration>();

        if (logger.IsEnabled(LogLevel.Information))
        {
            applicationLifetime.ApplicationStarted.Register(() =>
                logger.LogInformation("{ApplicationName} started", env.ApplicationName));
            applicationLifetime.ApplicationStopping.Register(() =>
                logger.LogInformation("{ApplicationName} stopping", env.ApplicationName));
            applicationLifetime.ApplicationStopped.Register(() =>
                logger.LogInformation("{ApplicationName} stopped", env.ApplicationName));
        }

        app.UseRouting();

        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapControllers().AllowAnonymous();

        app.MapGet("/", () => "Alive!").AllowAnonymous();
    }
}
