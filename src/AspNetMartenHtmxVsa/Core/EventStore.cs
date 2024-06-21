using AspNetMartenHtmxVsa.Features;
using Marten;
using static AspNetMartenHtmxVsa.Core.Constants;

namespace AspNetMartenHtmxVsa.Core;

public interface IGlobalStore : IDocumentStore;

public interface ISubscribersStore : IDocumentStore;

public interface IFreeUsersStore : IDocumentStore;

public static class EventStore
{
  public static string GetSubscriberEventStoreId(
    SubscriptionId subscriptionId
  )
  {
    if (subscriptionId is null || subscriptionId.Value == Guid.Empty)
      throw new ArgumentNullException(nameof(subscriptionId));
    return $"${AppNameShortLower}_{subscriptionId.Value.ToString().Replace("-", "_")}";
  }
  
  public static string GetDefaultEventStoreId(
    EventStoreConfiguration? configuration
  )
  {
    return string.IsNullOrEmpty(configuration?.DefaultEventstoreId)
      ? Constants.DefaultEventStore
      : configuration.DefaultEventstoreId;
  }
}
