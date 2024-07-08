using AspNetMartenHtmxVsa.Features.Subscriptions.Subscribe;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace AspNetMartenHtmxVsa.Features;

public static class SubscriptionsProjectionConfiguration
{
  public static void UseSubscriptionProjections(
    this StoreOptions options
  )
  {
    options.Projections.Add<SubscriptionProjection>(ProjectionLifecycle.Inline);
  }
}

public record Subscription(
  Guid Id,
  string CompanyName,
  string RegisteredBy,
  DateTimeOffset RegisteredOn
)
{
  public string? CompanyNameAddendum { get; set; }
  public string? AddressLine1 { get; set; }
  public string? AddressLine2 { get; set; }
  public string? ZipCode { get; set; }
  public string? City { get; set; }
}

public class SubscriptionProjection : SingleStreamProjection<Subscription>
{
  // Create a new aggregate based on the initial
  // event type
  // ReSharper disable once UnusedMember.Global
  public static Subscription Create(
    IEvent<Subscribed> subscribed
  )
  {
    return new Subscription(
      subscribed.StreamId,
      subscribed.Data.CompanyName,
      subscribed.Data.RegisteredBy,
      subscribed.Data.RegisteredOn
    );
  }
}
