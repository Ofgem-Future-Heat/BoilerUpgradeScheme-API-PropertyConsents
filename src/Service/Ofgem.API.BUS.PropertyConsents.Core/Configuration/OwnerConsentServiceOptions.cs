using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;

namespace Ofgem.API.BUS.PropertyConsents.Core.Configuration
{
    public class OwnerConsentServiceOptions : IOwnerConsentServiceOptions
    {
        private readonly string _consentEmailTemplateID;
        private readonly string _consentOwnerConfirmEmailTemplateID;
        private readonly string _consentInstallerConfirmEmailTemplateID;
        private readonly string _consentInstallerNotChosenEmailTemplateID;
        private readonly string _consentTokenSecret;

        public OwnerConsentServiceOptions(string consentEmailTemplateID, string consentOwnerConfirmEmailTemplateID, string consentInstallerConfirmEmailTemplateID, string consentInstallerNotChosenEmailTemplateID, string consentTokenSecret)
        {
            _consentEmailTemplateID = consentEmailTemplateID ?? throw new ArgumentNullException(nameof(consentEmailTemplateID));
            _consentOwnerConfirmEmailTemplateID = consentOwnerConfirmEmailTemplateID ?? throw new ArgumentNullException(nameof(consentOwnerConfirmEmailTemplateID));
            _consentInstallerConfirmEmailTemplateID = consentInstallerConfirmEmailTemplateID ?? throw new ArgumentNullException(nameof(consentInstallerConfirmEmailTemplateID));
            _consentInstallerNotChosenEmailTemplateID = consentInstallerNotChosenEmailTemplateID ?? throw new ArgumentNullException(nameof(consentInstallerNotChosenEmailTemplateID));
            _consentTokenSecret = consentTokenSecret ?? throw new ArgumentNullException(nameof(consentTokenSecret));
        }

        public string ConsentEmailTemplateID => _consentEmailTemplateID;

        public string ConsentTokenSecret => _consentTokenSecret;

        public string ConsentOwnerConfirmEmailTemplateID => _consentOwnerConfirmEmailTemplateID;

        public string ConsentInstallerNotChosenEmailTemplateID => _consentInstallerNotChosenEmailTemplateID;

        public string ConsentInstallerConfirmEmailTemplateID => _consentInstallerConfirmEmailTemplateID;
    }
}
