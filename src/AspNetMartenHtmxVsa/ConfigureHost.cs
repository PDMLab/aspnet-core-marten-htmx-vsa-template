using AspNetMartenHtmxVsa.Configuration;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.EventSourcing;
using AspNetMartenHtmxVsa.Features.Account;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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

            services
              .AddIdentity<AppUser, IdentityRole>(
                options => options.SignIn.RequireConfirmedAccount = false
              )
              .AddEntityFrameworkStores<AppDbContext>()
              .AddDefaultTokenProviders();

            services.AddTransient<IClaimsTransformation, OidcLikeClaimsTransformation>();
            
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();


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
                routeBuilder.MapFallbackToController("GetHome", "GetHome");
              }
            );
          }
        );
      }
    );
    return hostBuilder;
  }
}
