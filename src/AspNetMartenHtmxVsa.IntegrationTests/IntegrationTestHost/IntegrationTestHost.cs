using Alba;
using AspNetMartenHtmxVsa.Tests.TestSetup;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace AspNetMartenHtmxVsa.Tests.IntegrationTestHost;

public class TestConfiguration : Dictionary<string, string?>
{
  public IConfigurationRoot AsConfigurationRoot()
  {
    return new ConfigurationBuilder()
      .AddInMemoryCollection(this)
      .Build();
  }
}

public class IntegrationTestHost : IDisposable
{
  private IAlbaHost Host { get; init; }
  private TestEventStore EventStore { get; init; }
  
  private IntegrationTestHost(IAlbaHost host, TestEventStore eventStore)
  {
    Host = host;
    EventStore = eventStore;
  }

  public static async Task<IAlbaHost> InitializeAsync(
    ITestOutputHelper? testOutputHelper = null
  )
  {
    var testEventStore = await TestEventStore.InitializeAsync();
    var configuration = new TestConfiguration
    {
      ["ConnectionStrings:EventStore"] = testEventStore.MasterDbConnectionString
    }.AsConfigurationRoot();

    var builder = ConfigureHost.GetHostBuilder(configuration, services => {});

    return await builder.StartAlbaAsync();
  }
  
  public async Task DisposeAsync()
  {
    await Host.DisposeAsync();
    await EventStore.DisposeAsync();
  }

  public void Dispose()
  {
    DisposeAsync().GetAwaiter().GetResult();
  }
}
