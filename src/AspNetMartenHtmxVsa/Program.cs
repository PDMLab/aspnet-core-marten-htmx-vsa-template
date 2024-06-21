using AspNetMartenHtmxVsa;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationManager()
  .AddJsonFile("appsettings.json")
  .AddJsonFile("appsettings.Development.json")
  .Build();

var builder = ConfigureHost.GetHostBuilder(
  configuration,
  services => { }
);

builder.AddLogging();
builder
  .Build()
  .Run();
