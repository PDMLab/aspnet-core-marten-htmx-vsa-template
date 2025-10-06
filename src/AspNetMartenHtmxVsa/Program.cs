using AspNetMartenHtmxVsa;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.Features.SeedIdentity;
using Spectre.Console.Cli;

var configuration = new ConfigurationManager()
  .AddJsonFile("appsettings.json")
  .AddJsonFile("appsettings.Development.json")
  .Build();

var builder = ConfigureHost.GetHostBuilder(
  configuration,
  services => { }
);


builder.AddLogging();
var build = builder
  .Build();


if (args.Length > 0 && args[0]
      .StartsWith("seed"))
{
  var registrar = new TypeRegistrar(ConfigureHost.Services);
  var commandApp = new CommandApp(registrar);
  commandApp.Configure(c => { c.AddCommand<SeedIdentityCommand>("seed-identity"); });
  return await commandApp.RunAsync(args);
}


await build
  .RunAsync();
return 0;
