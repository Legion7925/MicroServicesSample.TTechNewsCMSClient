

using Serilog;
using Serilog.Sinks.Elasticsearch;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

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
    builder.Services.AddHttpClient("bi", c =>
    {
        c.BaseAddress = new Uri("http://localhost:7300/bi/");
    });
    builder.Services.AddHttpClient("news", c =>
    {
        c.BaseAddress = new Uri("http://localhost:7300/news/");
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

    app.UseAuthorization();
    app.MapHealthChecks("/health/live");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

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
