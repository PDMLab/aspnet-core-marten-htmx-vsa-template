using Marten;
using Marten.Events.Projections;

namespace AspNetMartenHtmxVsa.Features.Subscriptions.SetOrganizationAddress;

public static class RegisterOrganizationProjections
{
  public static void UseOrganizationProjections(
    this StoreOptions options
  )
  {
    options.Projections.Add<OrganizationProjection>(ProjectionLifecycle.Inline);
  }
}
