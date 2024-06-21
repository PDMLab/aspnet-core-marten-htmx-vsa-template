using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.Features.PrincipalExtensions;
using AspNetMartenHtmxVsa.Features.Subscriptions.SetCompanyNameFromUserRegistration;
using AspNetMartenHtmxVsa.Models;
using Marten.Events;
using Marten.Events.Aggregation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.SetOrganizationAddress;

[Authorize]
public class SetOrganizationAddressController : Controller
{
  [HttpGet]
  [Route("/settings/organization/address")]
  public async Task<IActionResult> SetOrganizationAddress(
    [FromServices] ISubscribersStore store
  )
  {
    var subscriptionId = User.GetSubscriptionId();
    var organizationId = OrganizationId.FromSubscription(subscriptionId);
    await using var session = store.QuerySession(EventStore.GetSubscriberEventStoreId(subscriptionId));
    var organization = session.Load<Organization>(organizationId.Value);

    if (organization == null)
      return View("Error", new ErrorViewModel(HttpContext, "Organisation kann nicht geladen werden."));

    var setOrgAddress = new SetOrgAddress(organization);

    return View(setOrgAddress);
  }


  [HttpPut]
  [Route("/settings/organization/address")]
  public async Task<IActionResult> SetOrganizationAddress(
    [FromForm] SetOrgAddress model,
    [FromServices] ISubscribersStore store
  )
  {
    if (!ModelState.IsValid) return View(model);

    var subscriptionId = User.GetSubscriptionId();

    await using var session =
      store.LightweightSession(EventStore.GetSubscriberEventStoreId(subscriptionId));

    var (companyName, companyNameAddendum, addressLine1, addressLine2, zipCode, city) = model;

    session.Events.Append(
      OrganizationId.FromSubscription(subscriptionId)
        .Value,
      new OrganizationAddressSet(
        companyName,
        companyNameAddendum,
        addressLine1!,
        addressLine2,
        zipCode!,
        city!
      )
    );

    await session.SaveChangesAsync();

    return View();
  }
}

public record Organization(
  Guid Id,
  string CompanyName,
  Guid InquiryFormId,
  string? CompanyNameAddendum,
  string? AddressLine1,
  string? AddressLine2,
  string? ZipCode,
  string? City,
  string? LeadInquiryWebFormReturnUrl,
  bool ReturnOnSuccess
  // ReSharper disable once NotAccessedPositionalProperty.Global
);

public class OrganizationProjection : SingleStreamProjection<Organization>
{
  // ReSharper disable once UnusedMember.Global
  public Organization Create(
    IEvent<CompanyNameSetFromUserRegistration> nameSetFromUserRegistration
  )
  {
    var companyName = nameSetFromUserRegistration.Data.CompanyName;
    return new Organization(
      nameSetFromUserRegistration.StreamId,
      companyName,
      Guid.Empty,
      null,
      null,
      null,
      null,
      null,
      string.Empty,
      false
    );
  }


  // ReSharper disable once UnusedMember.Global
  public Organization Apply(
    OrganizationAddressSet organizationAddressSet,
    Organization current
  )
  {
    var (companyName, companyNameAddendum, addressLine1, addressLine2, zipCode, city) = organizationAddressSet;
    return current with
    {
      CompanyName = companyName,
      CompanyNameAddendum = companyNameAddendum,
      AddressLine1 = addressLine1,
      AddressLine2 = addressLine2,
      ZipCode = zipCode,
      City = city
    };
  }
}

public record OrganizationAddressSet(
  string CompanyName,
  string? CompanyNameAddendum,
  string AddressLine1,
  string? AddressLine2,
  string ZipCode,
  string City
);

public class SetOrgAddress
{
  public SetOrgAddress() : this(
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty,
    string.Empty
  )
  {
  }

  public SetOrgAddress(
    Organization organization
  ) :
    this(
      organization.CompanyName,
      organization.CompanyNameAddendum,
      organization.AddressLine1,
      organization.AddressLine2,
      organization.ZipCode,
      organization.City
    )
  {
  }

  public SetOrgAddress(
    string companyName,
    string? companyNameAddendum,
    string? addressLine1,
    string? addressLine2,
    string? zipCode,
    string? city
  )
  {
    CompanyName = companyName;
    CompanyNameAddendum = companyNameAddendum;
    AddressLine1 = addressLine1;
    AddressLine2 = addressLine2;
    ZipCode = zipCode;
    City = city;
  }

  [DisplayName("Firmenname")]
  [Required(ErrorMessage = "Firmenname ist ein Pflichtfeld")]
  public string CompanyName { get; init; }

  [DisplayName("Zusatz Firmenname")] public string? CompanyNameAddendum { get; init; }

  [DisplayName("Straße und Hausnummer")]
  [Required(ErrorMessage = "Straße ist ein Pflichtfeld")]
  public string? AddressLine1 { get; init; }

  [DisplayName("Zusatz Adresse")] public string? AddressLine2 { get; init; }

  [DisplayName("PLZ")]
  [Required(ErrorMessage = "PLZ ist ein Pflichtfeld")]
  public string? ZipCode { get; init; }

  [DisplayName("Ort")]
  [Required(ErrorMessage = "Ort ist ein Pflichtfeld")]
  public string? City { get; init; }

  public void Deconstruct(
    out string companyName,
    out string? companyNameAddendum,
    out string? addressLine1,
    out string? addressLine2,
    out string? zipCode,
    out string? city
  )
  {
    companyName = CompanyName;
    companyNameAddendum = CompanyNameAddendum;
    addressLine1 = AddressLine1;
    addressLine2 = AddressLine2;
    zipCode = ZipCode;
    city = City;
  }
}
