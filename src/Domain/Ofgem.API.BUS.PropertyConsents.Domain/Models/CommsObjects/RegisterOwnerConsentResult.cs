namespace Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;

/// <summary>
/// Result of an attempt to register owner consent.
/// </summary>
public record RegisterOwnerConsentResult
{
    /// <summary>
    /// Flag to show if this consent request was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Flag to show if this consent request is ineligible to be completed.
    /// </summary>
    public bool? IsIneligible { get; set; }
}
