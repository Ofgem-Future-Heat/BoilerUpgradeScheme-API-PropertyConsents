using NUnit.Framework;
using System;
using FluentAssertions;
using Ofgem.API.BUS.PropertyConsents.Core.Configuration;

namespace Ofgem.API.BUS.PropertyConsents.Core.UnitTests
{
    [TestFixture]
    public class OwnerConsentServiceOptionsTests : BaseServiceTests
    {
        private OwnerConsentServiceOptions _systemUnderTest;

        [Test]
        public void Can_Be_Instantiated_With_Valid_Parameters()
        {
            // act
            _systemUnderTest = new OwnerConsentServiceOptions(
                "consentEmailTemplateID",
                "consentOwnerConfirmEmailTemplateID",
                "consentInstallerConfirmEmailTemplateID",
                "consentInstallerNotChosenEmailTemplateID",
                "consentTokenSecret");

            // assert
            _systemUnderTest.Should().NotBeNull();

            _systemUnderTest.ConsentEmailTemplateID.Should().Be("consentEmailTemplateID");
            _systemUnderTest.ConsentOwnerConfirmEmailTemplateID.Should().Be("consentOwnerConfirmEmailTemplateID");
            _systemUnderTest.ConsentInstallerConfirmEmailTemplateID.Should().Be("consentInstallerConfirmEmailTemplateID");
            _systemUnderTest.ConsentInstallerNotChosenEmailTemplateID.Should().Be("consentInstallerNotChosenEmailTemplateID");
            _systemUnderTest.ConsentTokenSecret.Should().Be("consentTokenSecret");
        }


        [TestCase("consentEmailTemplateID", null!, "consentOwnerConfirmEmailTemplateID", "consentInstallerConfirmEmailTemplateID", "consentInstallerNotChosenEmailTemplateID", "consentTokenSecret")]
        [TestCase("consentOwnerConfirmEmailTemplateID", "consentEmailTemplateID", null!, "consentInstallerConfirmEmailTemplateID", "consentInstallerNotChosenEmailTemplateID", "consentTokenSecret")]
        [TestCase("consentInstallerConfirmEmailTemplateID", "consentEmailTemplateID", "consentOwnerConfirmEmailTemplateID", null!, "consentInstallerNotChosenEmailTemplateID", "consentTokenSecret")]
        [TestCase("consentInstallerNotChosenEmailTemplateID", "consentEmailTemplateID", "consentOwnerConfirmEmailTemplateID", "consentInstallerConfirmEmailTemplateID", null!, "consentTokenSecret")]
        [TestCase("consentTokenSecret", "consentEmailTemplateID", "consentOwnerConfirmEmailTemplateID", "consentInstallerConfirmEmailTemplateID", "consentInstallerNotChosenEmailTemplateID", null!)]
        public void Cannot_Be_Instantiated_With_Invalid_Parameters(
            string paramUnderTest,
            string consentEmailTemplateID,
            string consentOwnerConfirmEmailTemplateID,
            string consentInstallerConfirmEmailTemplateID, 
            string consentInstallerNotChosenEmailTemplateID,
            string consentTokenSecret)
        {
            // act assert
            Action act = () => new OwnerConsentServiceOptions(
                consentEmailTemplateID,
                consentOwnerConfirmEmailTemplateID,
                consentInstallerConfirmEmailTemplateID,
                consentInstallerNotChosenEmailTemplateID,
                consentTokenSecret);

            act.Should().Throw<ArgumentNullException>().WithParameterName(paramUnderTest);
        }
    }
}