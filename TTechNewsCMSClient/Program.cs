

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.IdentityModel.Tokens.Jwt;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    const string serviceName = "TTechNewsCMSClient";

    builder.Logging.AddOpenTelemetry(options =>
    {
        options
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName))
            .AddConsoleExporter();
    });
    builder.Services.AddOpenTelemetry()
          .ConfigureResource(resource => resource.AddService(serviceName))
          .WithTracing(tracing => tracing
              .AddAspNetCoreInstrumentation()
              .AddConsoleExporter()
              .AddJaegerExporter()
              .AddSqlClientInstrumentation())
          .WithMetrics(metrics => metrics
              .AddAspNetCoreInstrumentation()
              .AddConsoleExporter());

    builder.Host.UseSerilog((ctx, lc) => lc
  .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
  {
      DetectElasticsearchVersion = false,
      AutoRegisterTemplate = true,
      AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
      IndexFormat = "ttech-newscmsclient-index-{0:yyyy.MM}",
      MinimumLogEventLevel = Serilog.Events.LogEventLevel.Debug
  })
   .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
   .Enrich.FromLogContext()
   .ReadFrom.Configuration(ctx.Configuration));

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
    builder.Services.AddAuthentication(c =>
    {
        c.DefaultScheme = "Cookies";
        c.DefaultChallengeScheme = "oidc";
    }).AddCookie("Cookies").AddOpenIdConnect("oidc" , c=>
    {
        c.Authority = "https://localhost:5001/";
        c.ClientId = "newscmsClient";
        c.ClientSecret = "newscmsClient";
        c.ResponseType = "code";
        c.Scope.Clear();
        c.Scope.Add("openid");
        c.Scope.Add("profile");
        c.Scope.Add("basicinfo");
        c.Scope.Add("newscms");
        c.Scope.Add("offline_access");
        c.GetClaimsFromUserInfoEndpoint = true;
        c.SaveTokens = true;
    });
    builder.Services.AddHttpClient("bi", c =>
    {
        c.BaseAddress = new Uri("http://localhost:7300/bi/");
    });
    builder.Services.AddHttpClient("news", c =>
    {
        c.BaseAddress = new Uri("http://localhost:7300/news/");
    });
    builder.Services.AddHttpClient("oAuth", c =>
    {
        c.BaseAddress = new Uri("https://localhost:5001/");
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
    builder.Services.AddHealthChecks();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health/live");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
