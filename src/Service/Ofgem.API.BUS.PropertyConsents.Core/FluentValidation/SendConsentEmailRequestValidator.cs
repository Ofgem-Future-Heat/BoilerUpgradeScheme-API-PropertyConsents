using FluentValidation;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;

namespace Ofgem.API.BUS.PropertyConsents.Core.FluentValidation
{
    public  class SendConsentEmailRequestValidator : AbstractValidator<SendConsentEmailRequest>
    {
        public SendConsentEmailRequestValidator()
        {
            RuleFor(x => x).NotNull();

            RuleFor(x => x.ApplicationReferenceNumber).NotEmpty();
            RuleFor(x => x.InstallerName).NotEmpty();
            RuleFor(x => x.TechnologyType).NotEmpty();
            RuleFor(x => x.ConsentRequestId).NotEmpty();
            RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress();
            RuleFor(x => x.ConsentRequestExpiryDays).NotEmpty().GreaterThan(0);
            RuleFor(x => x.InstallationAddressLine1).NotEmpty().MaximumLength(127);
            RuleFor(x => x.InstallationAddressLine2).MaximumLength(127);
            RuleFor(x => x.InstallationAddressLine3).MaximumLength(127);
            RuleFor(x => x.InstallationAddressCounty).MaximumLength(31);
            RuleFor(x => x.InstallationAddressPostcode).NotEmpty().MaximumLength(8);
        }
    }
}
