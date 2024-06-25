using Serilog;
using Serilog.Sinks.Elasticsearch;
using Steeltoe.Discovery.Client;
using TTechCMSApiGateway.Extentsions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc
 .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
   .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
   {
       DetectElasticsearchVersion = false,
       AutoRegisterTemplate = true,
       AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
       IndexFormat = "ttech-apigateway-index-{0:yyyy.MM}",
       MinimumLogEventLevel = Serilog.Events.LogEventLevel.Debug
   })
 .Enrich.FromLogContext()
 .ReadFrom.Configuration(ctx.Configuration));
    builder.Services.AddDiscoveryClient();
    builder.Services.AddHealthChecks();
    builder.Services.AddReverseProxy()
        .LoadFromEureka(builder.Services);
    //.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    var app = builder.Build();

    app.MapReverseProxy();
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/health/live");
    });

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