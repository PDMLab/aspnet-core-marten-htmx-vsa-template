namespace AspNetMartenHtmxVsa.Features.Subscriptions.Subscribe;

public record Subscribed(
  string CompanyName,
  string RegisteredBy,
  DateTimeOffset RegisteredOn,
  SubscriptionId? SubscriptionId
);
