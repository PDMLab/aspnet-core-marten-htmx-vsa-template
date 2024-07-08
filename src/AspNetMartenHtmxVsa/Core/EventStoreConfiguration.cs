using AspNetMartenHtmxVsa.Features;
using AspNetMartenHtmxVsa.Features.Subscriptions.RegisterUser;
using AspNetMartenHtmxVsa.Features.Subscriptions.SetOrganizationAddress;
using static AspNetMartenHtmxVsa.Core.Constants;
using Marten;
using Marten.Events.Projections;
using Marten.Storage;
using Weasel.Core;

namespace AspNetMartenHtmxVsa.Core;

// Implement IRetryPolicy interface

public class EventStoreConfiguration
{
  private const string DefaultSchema = AppNameShortLower;

  public string ConnectionString { get; set; } = default!;

  public string WriteModelSchema { get; set; } = DefaultSchema;
  public string ReadModelSchema { get; set; } = DefaultSchema;

  public bool ShouldRecreateDatabase { get; set; }

  // public bool UseMetadata = true;
  public string DefaultEventstoreId { get; set; } = AppNameShortLower;
}

public static class MartenConfigExtensions
{
  private const string DefaultConfigKey = "EventStore";

  public static IServiceCollection AddMarten(
    this IServiceCollection services,
    IConfiguration config,
    Action<StoreOptions>? configureGlobalStoreOptions = null,
    Action<StoreOptions>? configuresubscriberStoreOptions = null,
    Action<StoreOptions>? configureFreeUsersStoreOptions = null,
    string globalConfigKey = DefaultConfigKey,
    string freeUsersConfigKey = DefaultConfigKey,
    string subscriptionConfigKey = DefaultConfigKey
  )
  {
    var globalStoreConfigSection = config.GetSection(globalConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    var subscriptionConfigSection = config.GetSection(subscriptionConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    var freeUsersConfigSection = config.GetSection(freeUsersConfigKey)
      .Get<EventStoreConfiguration>() ?? throw new NullReferenceException();

    services.AddMartenStore<IGlobalStore>(
      _ => SetGlobalStoreOptions(globalStoreConfigSection, configureGlobalStoreOptions)
    );

    var martenStoreExpression = services
      .AddSingleton(subscriptionConfigSection)
      .AddMartenStore<ISubscribersStore>(
        _ => SetSubscriberStoreOptions(
          subscriptionConfigSection,
          configuresubscriberStoreOptions
        )
      );

    services.AddMartenStore<IFreeUsersStore>(
      _ =>
        SetFreeUsersStoreOptions(freeUsersConfigSection, configureFreeUsersStoreOptions)
    );

    return services;
  }

  private static StoreOptions SetGlobalStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var globalStoreOptions = new StoreOptions();
    globalStoreOptions.Connection(eventStoreConfiguration.ConnectionString);

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    globalStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    globalStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;

    globalStoreOptions.UseDefaultSerialization(
      nonPublicMembersStorage: NonPublicMembersStorage.All
    );

    globalStoreOptions.Schema.For<User>()
      .AddSubClass<RegisteredUser>()
      .AddSubClass<ActiveUser>();

    globalStoreOptions.Projections.Add<RegisteredUserProjection>(ProjectionLifecycle.Inline);
    globalStoreOptions.Projections.Add<ActiveUserProjection>(ProjectionLifecycle.Inline);
    globalStoreOptions.UseSubscriptionProjections();

    configureOptions?.Invoke(globalStoreOptions);

    return globalStoreOptions;
  }

  public static StoreOptions SetSubscriberStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var subscriberStoreOptions = new StoreOptions();
    // configure tenancy for database per tenant
    subscriberStoreOptions.MultiTenantedWithSingleServer(eventStoreConfiguration.ConnectionString);

    subscriberStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    subscriberStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    subscriberStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;

    // var connectionString =
    //   context.Configuration
    //     .GetSection("EventStore")["ConnectionString"] ??
    //   throw new InvalidOperationException();

    subscriberStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

    subscriberStoreOptions.UseOrganizationProjections();


    // subscriberStoreOptions.UseInquiryChatProjections();

    configureOptions?.Invoke(subscriberStoreOptions);

    return subscriberStoreOptions;
  }

  private static StoreOptions SetFreeUsersStoreOptions(
    EventStoreConfiguration eventStoreConfiguration,
    Action<StoreOptions>? configureOptions = null
  )
  {
    var freeUsersStoreOptions = new StoreOptions();
    freeUsersStoreOptions.Connection(eventStoreConfiguration.ConnectionString);

    // configure tenancy for tenantId column in the same database
    freeUsersStoreOptions.Policies.ForAllDocuments(x => x.TenancyStyle = TenancyStyle.Conjoined);
    freeUsersStoreOptions.Events.TenancyStyle = TenancyStyle.Conjoined;
    freeUsersStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

    var schemaName = Environment.GetEnvironmentVariable("SchemaName");
    freeUsersStoreOptions.Events.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.WriteModelSchema;
    freeUsersStoreOptions.DatabaseSchemaName = schemaName ?? eventStoreConfiguration.ReadModelSchema;

    freeUsersStoreOptions.UseNewtonsoftForSerialization(
      nonPublicMembersStorage: NonPublicMembersStorage.All
    );

    configureOptions?.Invoke(freeUsersStoreOptions);

    return freeUsersStoreOptions;
  }
}
