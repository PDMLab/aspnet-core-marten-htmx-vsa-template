using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Configuration;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.EventSourcing;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

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
          (
            context,
            services
          ) =>
          {
            var identityTestConnectionString = context.Configuration.GetConnectionString("Identity");
            services.AddDbContext<AppDbContext>(
              options => { options.UseNpgsql(identityTestConnectionString); }
            );

            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
              .AddEntityFrameworkStores<AppDbContext>();
            
            services.AddTransient<IClaimsTransformation, OidcLikeClaimsTransformation>();


            services.AddMarten(
              context.Configuration
            );


            services.Configure<RazorViewEngineOptions>(
              options =>
              {
                options.ViewLocationExpanders.Add(new FeatureFolderLocationExpander());
              }
            );
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllersWithViews()
              .AddRazorRuntimeCompilation();
            services.AddRazorPages();
            configureServices?.Invoke(services);
          }
        );
        builder.Configure(
          app =>
          {
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(
              routeBuilder =>
              {
                routeBuilder.MapControllers();
                routeBuilder.MapRazorPages();
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
