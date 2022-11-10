using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain.Entities;
using Ofgem.API.BUS.PropertyConsents.Core.FluentValidation;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;

namespace Ofgem.API.BUS.PropertyConsents.Core.UnitTests.FluentValidation
{
    [TestFixture]
    public class SendConsentEmailRequestValidatorTests
    {
        private readonly SendConsentEmailRequestValidator _validator = new();

        [Test]
        public void NoErrors()
        {
            var result = _validator.TestValidate(new SendConsentEmailRequest
            {
                EmailAddress = "josh.anderson@ofgem.gov.uk",
                ApplicationReferenceNumber = "APW12345678",
                InstallerName = "APW Enterprises",
                TechnologyType = "Air source heat pump",
                InstallationAddressLine1 = "1 Canada Square",
                InstallationAddressLine2 = "Canary Wharf",
                InstallationAddressLine3 = "London",
                InstallationAddressCounty = "County",
                InstallationAddressPostcode = "E14 5AA",
                ConsentRequestExpiryDays = 14,
                PropertyOwnerPortalBaseURL = new Uri("https://www.boiler-upgrade.gov.uk"),
                ConsentRequestId = new Guid("b4904d64-f868-4485-b9e7-96fe76073db6")
            });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Errors_Null_Empty()
        {
            var result = _validator.TestValidate(new SendConsentEmailRequest());

            result.Errors.Should().HaveCount(9);
            result.ShouldHaveValidationErrorFor(model => model.ApplicationReferenceNumber);
            result.ShouldHaveValidationErrorFor(model => model.InstallerName);
            result.ShouldHaveValidationErrorFor(model => model.TechnologyType);
            result.ShouldHaveValidationErrorFor(model => model.ConsentRequestId);
            result.ShouldHaveValidationErrorFor(model => model.EmailAddress);
            result.ShouldHaveValidationErrorFor(model => model.ConsentRequestExpiryDays);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressLine1);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressPostcode);

            result.ShouldNotHaveValidationErrorFor(model => model.InstallationAddressLine2);
            result.ShouldNotHaveValidationErrorFor(model => model.InstallationAddressLine3);
            result.ShouldNotHaveValidationErrorFor(model => model.InstallationAddressCounty);
        }

        [Test]
        public void Errors_Formats()
        {
            var result = _validator.TestValidate(new SendConsentEmailRequest
            {
                EmailAddress = "josh.uk",
                ApplicationReferenceNumber = "APW12345678",
                InstallerName = "APW Enterprises",
                TechnologyType = "Air source heat pump",
                InstallationAddressLine1 = "1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square  Canada Square",
                InstallationAddressLine2 = "1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square  Canada Square",
                InstallationAddressLine3 = "1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square 1 Canada Square  Canada Square",
                InstallationAddressCounty = "1 Canada Square 1 Canada Square 1 Canada Square",
                InstallationAddressPostcode = "E14 5AAAA",
                ConsentRequestExpiryDays = 0,
                PropertyOwnerPortalBaseURL = new Uri("https://www.boiler-upgrade.gov.uk"),
                ConsentRequestId = new Guid("b4904d64-f868-4485-b9e7-96fe76073db6")
            });

            result.Errors.Should().HaveCount(8);
            result.ShouldNotHaveValidationErrorFor(model => model.ApplicationReferenceNumber);
            result.ShouldNotHaveValidationErrorFor(model => model.InstallerName);
            result.ShouldNotHaveValidationErrorFor(model => model.TechnologyType);
            result.ShouldNotHaveValidationErrorFor(model => model.ConsentRequestId);

            result.ShouldHaveValidationErrorFor(model => model.ConsentRequestExpiryDays);
            result.ShouldHaveValidationErrorFor(model => model.EmailAddress);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressLine1);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressLine2);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressLine3);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressCounty);
            result.ShouldHaveValidationErrorFor(model => model.InstallationAddressPostcode);
        }
    }
}
