using Microsoft.IdentityModel.Tokens;
using Notify.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ofgem.API.BUS.PropertyConsents.Core;

/// <summary>
/// IOwnerConsentService implementation with functions for Owner Consent operations
/// </summary>
public class OwnerConsentService : IOwnerConsentService
{
    #region Class Setup
    private readonly IOwnerConsentServiceOptions _ownerConsentServiceOptions;
    private readonly IAsyncNotificationClient _govNotifyClient;

    public OwnerConsentService(IOwnerConsentServiceOptions ownerConsentServiceOptions, IAsyncNotificationClient govNotifyClient)
    {
        _ownerConsentServiceOptions = ownerConsentServiceOptions ?? throw new ArgumentNullException(nameof(ownerConsentServiceOptions));
        _govNotifyClient = govNotifyClient ?? throw new ArgumentNullException(nameof(govNotifyClient));
    }

    #endregion

    #region Interface Implementation

    public async Task<SendConsentEmailResult> SendConsentEmailAsync(SendConsentEmailRequest request)
    {
        var tokenExpiryDate = DateTime.UtcNow.AddDays(request.ConsentRequestExpiryDays);
        var timeSpan = new TimeSpan(23, 59, 59);
        tokenExpiryDate = tokenExpiryDate.Date + timeSpan;

        var emailPersonalisation = GetEmailPersonalisationObject(request, tokenExpiryDate);

        try
        {
            _ = await _govNotifyClient.SendEmailAsync(
                request.EmailAddress,
                _ownerConsentServiceOptions.ConsentEmailTemplateID,
                emailPersonalisation,
                null,
                null
            );

        }
        catch (Exception)
        {
            return new SendConsentEmailResult
            {
                IsSuccess = false
            };
        }


        return new SendConsentEmailResult
        {
            IsSuccess = true,
            ConsentRequestId = request.ConsentRequestId,
            ConsentTokenExpires = tokenExpiryDate
        };

    }

    public async Task<SendConsentConfirmationEmailResult> SendConsentConfirmationEmailAsync(SendConsentConfirmationEmailRequest request)
    {
        var confirmEmailPersonalisation = GetConfirmEmailPersonalisationObject(request);

        try
        {
            _ = await _govNotifyClient.SendEmailAsync(
                request.OwnerEmailAddress,
                _ownerConsentServiceOptions.ConsentOwnerConfirmEmailTemplateID,
                confirmEmailPersonalisation,
                null,
                null
            );

        }
        catch (Exception)
        {
            return new SendConsentConfirmationEmailResult
            {
                IsSuccess = false
            };
        }

        try
        {
            _ = await _govNotifyClient.SendEmailAsync(
                request.InstallerEmailAddress,
                _ownerConsentServiceOptions.ConsentInstallerConfirmEmailTemplateID,
                confirmEmailPersonalisation,
                null,
                null
            );

        }
        catch (Exception)
        {
            return new SendConsentConfirmationEmailResult
            {
                IsSuccess = false
            };
        }

        return new SendConsentConfirmationEmailResult
        {
            IsSuccess = true,
            ConsentRequestId = request.ConsentRequestId
        };
    }

    public async Task<SendInstallersNotChosenEmailResult> SendRejectionEmailToInstallersNotChosenAsync(SendInstallersNotChosenEmailRequest request)
    {
        var notChosenEmailPersonalisation = GetInstallersNotChosenPersonalisationObject(request);
        try
        {
            _ = await _govNotifyClient.SendEmailAsync(
                request.InstallerEmailAddress,
                _ownerConsentServiceOptions.ConsentInstallerNotChosenEmailTemplateID,
                notChosenEmailPersonalisation,
                null,
                null
            );

        }
        catch (Exception)
        {
            return new SendInstallersNotChosenEmailResult
            {
                IsSuccess = false
            };
        }

        return new SendInstallersNotChosenEmailResult
        {
            IsSuccess = true
        };
    }

    public TokenVerificationResult VerifyToken(string token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        if (!ValidateToken(token))
        {
            return new TokenVerificationResult()
            {
                TokenAccepted = false
            };
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        if (securityToken == null)
        {
            return new TokenVerificationResult()
            {
                TokenAccepted = false
            };
        }

        return new TokenVerificationResult()
        {
            TokenAccepted = true,
            ConsentRequestId = Guid.Parse(securityToken.Claims.First(claim => claim.Type == "ConsentRequestId").Value),
            TokenExpiryDate = DateTime.Parse(securityToken.Claims.First(claim => claim.Type == "ConsentRequestExpiryDate").Value)
        };
    }

    #endregion

    #region Helpers

    private Dictionary<string, dynamic> GetEmailPersonalisationObject(SendConsentEmailRequest request, DateTime tokenExpiryDate)
    {
        DateTime localTokenExpiryDate = tokenExpiryDate.ToLocalTime();

        var multilineAddress = BuildMultiLineAddress(
            request.InstallationAddressLine1,
            request.InstallationAddressLine2,
            request.InstallationAddressLine3,
            request.InstallationAddressCounty,
            request.InstallationAddressPostcode);

        return new Dictionary<string, dynamic>()
        {
            {"ApplicationReferenceNumber", request.ApplicationReferenceNumber },
            {"InstallerName", request.InstallerName },
            {"TechnologyType", request.TechnologyType },
            {"MultilineAddress", multilineAddress },
            {"ServiceLevelAgreementDate", $"{localTokenExpiryDate:dd MMMM yyyy}" },
            {"PropertyOwnerConsentURL", $"{request.PropertyOwnerPortalBaseURL}verify?token={GenerateToken(request, tokenExpiryDate)}" }
        };
    }

    private Dictionary<string, dynamic> GetConfirmEmailPersonalisationObject(SendConsentConfirmationEmailRequest request)
    {
        var multilineAddress = BuildMultiLineAddress(
            request.InstallationAddressLine1,
            request.InstallationAddressLine2,
            request.InstallationAddressLine3,
            request.InstallationAddressCounty,
            request.InstallationAddressPostcode);

        return new Dictionary<string, dynamic>()
        {
            {"ApplicationReferenceNumber", request.ApplicationReferenceNumber },
            {"InstallerName", request.InstallerName },
            {"TechnologyType", request.TechnologyType },
            {"DisplayFriendlySingleLineInstallationAddress", multilineAddress },
            {"MultiLineInstallationAddress", multilineAddress },
            {"MultilineAddress", multilineAddress },
            {"Postcode", request.InstallationAddressPostcode }
        };
    }

    private Dictionary<string, dynamic> GetInstallersNotChosenPersonalisationObject(SendInstallersNotChosenEmailRequest request)
    {
        var multilineAddress = BuildMultiLineAddress(
            request.InstallationAddressLine1,
            request.InstallationAddressLine2,
            request.InstallationAddressLine3,
            request.InstallationAddressCounty,
            request.InstallationAddressPostcode);

        return new Dictionary<string, dynamic>()
        {
            {"InstallerEmail" , request.InstallerEmailAddress},
            {"TechnologyType", request.TechnologyType },
            {"MultilineAddress", multilineAddress },
            {"Postcode", request.InstallationAddressPostcode }
        };
    }

    private string GenerateToken(SendConsentEmailRequest request, DateTime tokenExpiryDate)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_ownerConsentServiceOptions.ConsentTokenSecret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("ConsentRequestId", request.ConsentRequestId.ToString()),
                new Claim("ConsentRequestExpiryDate", tokenExpiryDate.ToString())
            }),
            Expires = tokenExpiryDate,
            SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private bool ValidateToken(string token)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_ownerConsentServiceOptions.ConsentTokenSecret));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = mySecurityKey,
                ValidateAudience = false,
                ValidateIssuer = false
            }, out SecurityToken validatedToken);
        }
        catch
        {
            return false;
        }
        return true;
    }
    private string BuildMultiLineAddress(string installationAddressLine1, string installationAddressLine2, string? installationAddressLine3, string installationAddressCounty, string installationAddressPostcode)
    {
        string multilineAddress = string.Empty;
        if (!string.IsNullOrWhiteSpace(installationAddressLine1)) { multilineAddress += $"{installationAddressLine1}\n"; }
        if (!string.IsNullOrWhiteSpace(installationAddressLine2)) { multilineAddress += $"{installationAddressLine2}\n"; }
        if (!string.IsNullOrWhiteSpace(installationAddressLine3)) { multilineAddress += $"{installationAddressLine3}\n"; }
        if (!string.IsNullOrWhiteSpace(installationAddressCounty)) { multilineAddress += $"{installationAddressCounty}\n"; }
        if (!string.IsNullOrWhiteSpace(installationAddressPostcode)) { multilineAddress += $"{installationAddressPostcode}\n"; }
        multilineAddress = multilineAddress.TrimEnd(Environment.NewLine.ToCharArray());

        return multilineAddress;
    }

    #endregion
}