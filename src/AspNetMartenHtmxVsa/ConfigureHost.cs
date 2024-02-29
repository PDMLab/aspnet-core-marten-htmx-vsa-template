using AspNetMartenHtmxVsa.EventSourcing;
using Marten;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;

namespace AspNetMartenHtmxVsa;

public class ConfigureHost
{
  public static IHostBuilder GetHostBuilder(
    IConfigurationRoot configuration,
    Action<IServiceCollection>? configureServices = null
  )
  {
    var hostBuilder = Host.CreateDefaultBuilder();
    hostBuilder.ConfigureWebHostDefaults(
      builder =>
      {
        builder.ConfigureServices(
          services =>
          {
            services.Configure<RazorViewEngineOptions>(
              options => options.ViewLocationExpanders.Add(new FeatureFolderLocationExpander())
            );
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllersWithViews()
              .AddRazorRuntimeCompilation();
            services.AddMarten(
              options =>
              {
                options.Connection(configuration.GetConnectionString("eventstore"));
                StoreConfiguration.Configure(options);
              }
            );
            configureServices?.Invoke(services);
          }
        );
        builder.Configure(
          app =>
          {
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(
              routeBuilder =>
              {
                routeBuilder.MapControllers();
                routeBuilder.MapDefaultControllerRoute();
              }
            );
          }
        );
      }
    );
    return hostBuilder;
  }
}
