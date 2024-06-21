using AspNetMartenHtmxVsa.Core;
using static AspNetMartenHtmxVsa.Core.Guids;

namespace AspNetMartenHtmxVsa.Features;

public record CustomerNumber : StronglyTypedId<int>
{
  public CustomerNumber(
    int value
  ) : base(value)
  {
    if (value == 0) throw new ArgumentException(nameof(value));
  }
}

public record CustomerRelationId : StronglyTypedId<Guid>
{
  public CustomerRelationId(
    Guid value
  ) : base(value)
  {
    if (value == Guid.Empty) throw new ArgumentException(nameof(value));
  }

  public static CustomerRelationId Empty => new(Guid.Empty);
}

public record EmployeeId : StronglyTypedId<Guid>
{
  public EmployeeId(
    Guid value
  ) : base(value)
  {
    if (value == Guid.Empty) throw new ArgumentException(nameof(value));
  }

  public static EmployeeId FromUserId(
    UserId userId
  )
  {
    return new EmployeeId(userId.Value);
  }
}

public record UserId : StronglyTypedId<Guid>
{
  public UserId(
    Guid value
  ) : base(value)
  {
    if (value == Guid.Empty) throw new ArgumentException(nameof(value));
  }
}

public record SubscriptionId : StronglyTypedId<Guid>
{
  public SubscriptionId(
    Guid value
  ) : base(value)
  {
    if (value == Guid.Empty) throw new ArgumentException(nameof(value));
  }

  public static SubscriptionId FromOrganizationId(
    OrganizationId organizationId
  )
  {
    return new SubscriptionId(organizationId.Value);
  }

  public static SubscriptionId Anonymous => new(new Guid(AnonymousSubscriptionId));
}

public record OrganizationId : StronglyTypedId<Guid>
{
  public OrganizationId(
    Guid value
  ) : base(value)
  {
    if (value == Guid.Empty) throw new ArgumentException(nameof(value));
  }

  public static OrganizationId FromSubscription(
    SubscriptionId subscriptionId
  )
  {
    return new OrganizationId(subscriptionId.Value);
  }
}
