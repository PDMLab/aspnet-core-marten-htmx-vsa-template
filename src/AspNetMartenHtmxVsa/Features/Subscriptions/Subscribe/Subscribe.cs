namespace AspNetMartenHtmxVsa.Features.Subscribe;

public record Subscribed(
  string CompanyName,
  string RegisteredBy,
  DateTimeOffset RegisteredOn,
  SubscriptionId? SubscriptionId
);
