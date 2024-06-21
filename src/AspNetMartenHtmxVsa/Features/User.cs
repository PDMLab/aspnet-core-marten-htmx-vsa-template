namespace AspNetMartenHtmxVsa.Features;

public abstract record User
{
  public User(
    Guid id,
    string username,
    string email,
    string firstName,
    string lastName,
    string initials,
    Guid subscriptionId,
    string sub
  )
  {
    Id = id;
    Username = username;
    Email = email;
    FirstName = firstName;
    LastName = lastName;
    Initials = initials;
    SubscriptionId = subscriptionId;
    Sub = sub;
  }

  public Guid Id { get; init; }
  public string Username { get; set; } = null!;
  public string Email { get; protected init; } = null!;
  public string FirstName { get; protected init; } = null!;
  public string LastName { get; protected init; } = null!;
  public string Initials { get; protected init; } = null!;
  public Guid SubscriptionId { get; protected init; }
  public string Sub { get; protected init; } = null!;
  public string Fullname => $"{FirstName} {LastName}";
}
