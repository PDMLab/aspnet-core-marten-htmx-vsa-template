using System.ComponentModel;
using AspNetMartenHtmxVsa.Features.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AspNetMartenHtmxVsa.Features.SeedIdentity;

public sealed class TypeRegistrar : ITypeRegistrar
{
  private readonly IServiceCollection _services;

  public TypeRegistrar(
    IServiceCollection services
  ) => _services = services;

  public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());

  public void Register(
    Type service,
    Type implementation
  ) => _services.AddSingleton(service, implementation);

  public void RegisterInstance(
    Type service,
    object implementation
  ) => _services.AddSingleton(service, implementation);

  public void RegisterLazy(
    Type service,
    Func<object> factory
  ) => _services.AddSingleton(service, _ => factory());
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
  private readonly ServiceProvider _provider;

  public TypeResolver(
    ServiceProvider provider
  ) => _provider = provider;

  public object? Resolve(
    Type type
  ) => _provider.GetService(type);

  public void Dispose() => _provider.Dispose();
}

public sealed class SeedIdentityCommand : AsyncCommand<SeedIdentityCommand.Settings>
{
  private readonly IServiceProvider _sp;

  public SeedIdentityCommand(
    IServiceProvider sp
  ) => _sp = sp;

  public sealed class Settings : CommandSettings
  {
    [CommandOption("--apply-migrations")]
    [Description("Apply EF Core migrations before seeding")]
    public bool ApplyMigrations { get; set; } = true;

    [CommandOption("--user-id")] public string UserId { get; set; } = "4fd39017-24a5-4399-86d5-0732e22e9ac1";

    [CommandOption("--role-id")] public string RoleId { get; set; } = "3b6234bd-61d1-4d59-b660-1b0ed62ec9c4";

    [CommandOption("--role-name")] public string RoleName { get; set; } = "Administrator";

    [CommandOption("--username")] public string UserName { get; set; } = "admin@tempuri.org";

    [CommandOption("--email")] public string Email { get; set; } = "admin@tempuri.org";

    [CommandOption("--password")] public string Password { get; set; } = "ChangeMe_123!";
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context,
    Settings s
  )
  {
    using var scope = _sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    AnsiConsole.MarkupLine("[blue]Seeding Identity data[/]");

    // log settings
    AnsiConsole.MarkupLine($"  [grey]ApplyMigrations:[/] {s.ApplyMigrations}");
    AnsiConsole.MarkupLine($"  [grey]UserId:[/] {s.UserId}");
    AnsiConsole.MarkupLine($"  [grey]RoleId:[/] {s.RoleId}");
    AnsiConsole.MarkupLine($"  [grey]RoleName:[/] {s.RoleName}");
    AnsiConsole.MarkupLine($"  [grey]UserName:[/] {s.UserName}");
    AnsiConsole.MarkupLine($"  [grey]Email:[/] {s.Email}");
    AnsiConsole.MarkupLine($"  [grey]Password:[/] {new string('*', s.Password.Length)}");


    if (s.ApplyMigrations)
    {
      await db.Database.MigrateAsync();
      AnsiConsole.MarkupLine("[green]Migrations applied[/]");
    }

    // ensure role
    if (await roleMgr.FindByNameAsync(s.RoleName) == null)
    {
      var role = new IdentityRole
      {
        Id = s.RoleId,
        Name = s.RoleName,
        NormalizedName = s.RoleName.ToUpperInvariant()
      };
      await roleMgr.CreateAsync(role);
      AnsiConsole.MarkupLine($"[green]Role {s.RoleName} created[/]");
    }

    // ensure user
    var user = await userMgr.FindByIdAsync(s.UserId)
               ?? await userMgr.FindByNameAsync(s.UserName);
    if (user == null)
    {
      user = new AppUser()
      {
        Id = s.UserId,
        UserName = s.UserName,
        NormalizedUserName = s.UserName.ToUpperInvariant(),
        Email = s.Email,
        NormalizedEmail = s.Email.ToUpperInvariant(),
        EmailConfirmed = true,
        FirstName = "Admin",
        LastName = "User"
      };
      var result = await userMgr.CreateAsync(user, s.Password);
      if (!result.Succeeded)
      {
        foreach (var e in result.Errors) AnsiConsole.MarkupLine($"[red]{e.Code}: {e.Description}[/]");
        return -1;
      }

      AnsiConsole.MarkupLine($"[green]User {s.UserName} created[/]");
    }

    // add to role
    if (!await userMgr.IsInRoleAsync(user, s.RoleName))
    {
      await userMgr.AddToRoleAsync(user, s.RoleName);
      AnsiConsole.MarkupLine($"[green]User {s.UserName} added to role {s.RoleName}[/]");
    }

    return 0;
  }
}
