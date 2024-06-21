using System.Diagnostics;

namespace AspNetMartenHtmxVsa.Models;

public class ErrorViewModel
{
  public ErrorViewModel(
    HttpContext context
  ) : this(context, "Es ist ein Fehler aufgetreten.")
  {
  }


  public ErrorViewModel(
    HttpContext context,
    string? message
  )
  {
    RequestId = Activity.Current?.Id ?? context.TraceIdentifier;
    Message = message;
  }

  public ErrorViewModel()
  {
    RequestId = Activity.Current?.Id;
  }

  public string? Message { get; private set; }

  public string? RequestId { get; set; }

  public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
