using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using System;
using System.Collections.Generic;

namespace Ofgem.API.BUS.PropertyConsents.Core.UnitTests;

public abstract class BaseServiceTests
{
    protected Application GenerateApplication()
    {
        var application = new Application
        {
            ID = Guid.NewGuid(),
            BusinessAccountId = Guid.NewGuid(),
            QuoteAmount = 9000,
            TechTypeId = Guid.NewGuid(),
            ReferenceNumber = "12345",
            SubStatus = new ApplicationSubStatus
            {
                Code = ApplicationSubStatus.ApplicationSubStatusCode.SUB
            },
            InstallationAddress = new InstallationAddress
            {
                AddressLine1 = "line 1",
                AddressLine2 = "line 2",
                Postcode = "AB12 3CD",
                UPRN = "123456"
            },
            PropertyOwnerDetail = new PropertyOwnerDetail
            {
                Email = "chester@tester.com",
                FullName = "Chester Tester"
            },
            ConsentRequests = new List<ConsentRequest>()
        };

        return application;
    }

    protected ConsentRequestSummary GenerateConsentRequestSummary(Application? application)
    {
        application ??= GenerateApplication();

        var consentRequestSummary = new ConsentRequestSummary
        {
            ApplicationReferenceNumber = application.ReferenceNumber,
            ExpiryDate = DateTime.Now.AddDays(14),
            InstallationAddressCounty = application.InstallationAddress?.County ?? string.Empty,
            InstallationAddressLine1 = application.InstallationAddress?.AddressLine1 ?? string.Empty,
            InstallationAddressLine2 = application.InstallationAddress?.AddressLine2 ?? string.Empty,
            InstallationAddressLine3 = application.InstallationAddress?.AddressLine3 ?? string.Empty,
            InstallationAddressPostcode = application.InstallationAddress?.Postcode ?? string.Empty,
            InstallationAddressUprn = application.InstallationAddress?.UPRN ?? string.Empty,
            InstallerName = "Test Installer",
            InstallerEmailId = "test@test.com",
            OwnerEmailId = application.PropertyOwnerDetail?.Email ?? string.Empty,
            OwnerFullName = application.PropertyOwnerDetail?.FullName ?? string.Empty,
            QuoteAmount = application.QuoteAmount ?? 0,
            ServiceLevelAgreementDate = DateTime.Now.AddDays(14),
            TechnologyType = "Air source heat pump"
        };

        return consentRequestSummary;
    }
}
