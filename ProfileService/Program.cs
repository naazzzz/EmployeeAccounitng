using DotNetEnv;
using DotNetEnv.Configuration;
using General.Configurations;
using General.Interfaces;
using General.Service;
using ProfileService.Configurations;
using Serilog;
using LoadOptions = DotNetEnv.LoadOptions;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.AddConsole();
var configuration = builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath()).Build();

ServiceConfiguration.ConfigureServices(builder.Services, configuration);
SwaggerConfiguration.AddSwaggerOptions(builder);
RateLimiterConfiguration.Configure(builder.Services);

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    SwaggerConfiguration.AddDevelopSwaggerOptions(builder, app);
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseStaticFiles();
app.UseStatusCodePages();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapStaticAssets();

app.Run();