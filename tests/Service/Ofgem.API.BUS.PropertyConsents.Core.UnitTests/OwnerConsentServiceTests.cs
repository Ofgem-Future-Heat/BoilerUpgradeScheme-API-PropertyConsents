using Microsoft.IdentityModel.Tokens;
using Moq;
using Notify.Interfaces;
using NUnit.Framework;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Ofgem.API.BUS.PropertyConsents.Core.UnitTests
{
    [TestFixture]
    public class OwnerConsentServiceTests : BaseServiceTests
    {
        private OwnerConsentService _systemUnderTest;

        private readonly SendConsentEmailRequest _sendConsentEmailRequest = new()
        {
            EmailAddress = "josh.anderson@ofgem.gov.uk",
            ApplicationReferenceNumber = "APW12345678",
            InstallerName = "APW Enterprises",
            TechnologyType = "Air source heat pump",
            InstallationAddressLine1 = "1 Canada Square",
            InstallationAddressLine2 = "Canary Wharf",
            InstallationAddressLine3 = "London",
            InstallationAddressCounty = "",
            InstallationAddressPostcode = "E14 5AA",
            ConsentRequestExpiryDays = 14,
            PropertyOwnerPortalBaseURL = new Uri("https://www.boiler-upgrade.gov.uk"),
            ConsentRequestId = new Guid("b4904d64-f868-4485-b9e7-96fe76073db6")
        };

        private readonly SendInstallersNotChosenEmailRequest _sendRejectionEmailRequest = new()
        {
            InstallerEmailAddress = "josh.anderson@ofgem.gov.uk",
            InstallerName = "Josh Anderson", 
            TechnologyType = "Air source heat pump",
            InstallationAddressLine1 = "1 Canada Square",
            InstallationAddressLine2 = "Canary Wharf",
            InstallationAddressLine3 = "London",
            InstallationAddressCounty = "Test Country",
            InstallationAddressPostcode = "E14 5AA"
        };

        private readonly SendInstallersNotChosenEmailRequest _sendInstallersNotChosenEmailRequest = new()
        {
            InstallerEmailAddress = null,
            InstallerName = "Josh Anderson",
            TechnologyType = "Air source heat pump",
            InstallationAddressLine1 = "1 Canada Square",
            InstallationAddressLine2 = "Canary Wharf",
            InstallationAddressLine3 = "London",
            InstallationAddressCounty = "Test Country",
            InstallationAddressPostcode = "E14 5AA"
        };

        private readonly SendConsentConfirmationEmailRequest _sendConsentConfirmRequest = new SendConsentConfirmationEmailRequest
        {
            ConsentRequestId = Guid.NewGuid(),
            OwnerEmailAddress = "avinash.shinde@ofgem.gov.uk",
            InstallerEmailAddress = "avinash.shinde2@ofgem.gov.uk",
            ApplicationReferenceNumber = "123444",
            InstallerName = "Test name",
            TechnologyType = "Test TechnologyType",
            InstallationAddressLine1 = "Address 1",
            InstallationAddressLine2 = "Address 2",
            InstallationAddressLine3 = "Address 3",
            InstallationAddressCounty = "Test Country",
            InstallationAddressPostcode = "ER3 5RT"
        };

        private readonly SendConsentConfirmationEmailRequest _sendInvalidConsentConfirmRequest = new SendConsentConfirmationEmailRequest
        {
            ConsentRequestId = Guid.NewGuid(),
            OwnerEmailAddress = null,
            InstallerEmailAddress = null,
            ApplicationReferenceNumber = "123444",
            InstallerName = "Test name",
            TechnologyType = "Test TechnologyType",
            InstallationAddressLine1 = "Address 1",
            InstallationAddressLine2 = "Address 2",
            InstallationAddressLine3 = "Address 3",
            InstallationAddressCounty = "Test Country",
            InstallationAddressPostcode = "ER3 5RT"
        };

        Mock<IAsyncNotificationClient>? _mockGovNotifyClient = new();
        Mock<IOwnerConsentServiceOptions>? _mockOwnerConsentServiceOptions = new();

        private string _fakeGovNotifyTemplateId = "FakeGovNotifyTemplateId";
        private string _fakeGovNotifyConsentTokenSecret = "FakeGovNotifyConsentTokenSecret";
        private string _tokenExpiryDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd HH:mm:ss");
        private string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJDb25zZW50UmVxdWVzdElkIjoiYjQ5MDRkNjQtZjg2OC00NDg1LWI5ZTctOTZmZTc2MDczZGI2IiwiQ29uc2VudFJlcXVlc3RFeHBpcnlEYXRlIjoiMjEvMTEvMjAyMiAwOToxMzoxMSIsIm5iZiI6MTY0NTc4MzU2MSwiZXhwIjoxNjY5MDIxOTkxLCJpYXQiOjE2NDU3ODM1NjF9.aoXOpJn7S9HCxB_YA52cJfRLg696oPvdPgtPPIYxLQA";
        private string _invalidToken = "ThisCannotPossiblyBeAValidToken";

        #region Constructor Tests

        [Test]
        public void Constructor_Throws_ArgumentNullException_If_IOwnerConsentServiceOptions_Is_Null()
        {
            //assert & act
            Action act = () => _systemUnderTest = new OwnerConsentService(null!, _mockGovNotifyClient!.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("ownerConsentServiceOptions");
        }


        [Test]
        public void Constructor_Throws_ArgumentNullException_If_INotificationClient_Is_Null()
        {

            //assert & act
            Action act = () => _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions!.Object, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("govNotifyClient");    
        }

        [Test]
        public void Can_Be_Instantiated_With_Valid_Parameters()
        {
            // act
            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions!.Object, _mockGovNotifyClient!.Object);                      

            // assert

            var result = _systemUnderTest != null;

            result.Should().BeTrue();
        }

        #endregion

        #region Interface Implementation Tests

        #region SendConsentEmailAsync Tests

        [Test]
        public async Task SendConsentEmailAsync_Calls_NotificationClient_SendEmailAsync_Throws()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendConsentEmailRequest.EmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null)).Throws<Exception>();

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentEmailAsync(_sendConsentEmailRequest);

            // can't validate the dictionary because the token generated can vary for the same data, hence It.IsAny<Dictionary<string, dynamic>>()
            // assert
            testResult.Should().BeEquivalentTo(new SendConsentEmailResult { IsSuccess = false});
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentEmailRequest.EmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Once);
        }

        [Test]
        public async Task SendConsentEmailAsync_Calls_NotificationClient_SendEmailAsync_Once_Given_A_Valid_Request()
        {
            // arrange
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentEmailAsync(_sendConsentEmailRequest);

            // can't validate the dictionary because the token generated can vary for the same data, hence It.IsAny<Dictionary<string, dynamic>>()
            // assert
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentEmailRequest.EmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Once);
        }

        #endregion

        #region SendConfirmationConsentEmailAsync Tests

        [Test]
        public async Task SendConfirmationConsentEmailAsync_Calls_NotificationClient_SendEmailAsync_Twice_Given_A_Valid_Request()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentOwnerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentConfirmationEmailAsync(_sendConsentConfirmRequest);

            // assert
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.OwnerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(1));
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(1));
        }

        [Test]
        public async Task SendConfirmationConsentEmailAsync_Calls_NotificationClient_SendsNoEmailsAsync_Given_A_Missing_EmailAddress_In_Request()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentOwnerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentConfirmationEmailAsync(_sendInvalidConsentConfirmRequest);

            // assert
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.OwnerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Never());
        }


        [Test]
        public async Task SendConfirmationConsentEmailAsync_Calls_NotificationClient_SendEmailAsync_Owner_Pass_Installer_Throws()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentOwnerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendConsentConfirmRequest.OwnerEmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentOwnerConfirmEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null));

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendConsentConfirmRequest.InstallerEmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentInstallerConfirmEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null)).Throws<Exception>();

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentConfirmationEmailAsync(_sendConsentConfirmRequest);

            // assert
            testResult.Should().BeEquivalentTo(new SendConsentConfirmationEmailResult { IsSuccess = false });
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.OwnerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(1));
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(1));
        }

        [Test]
        public async Task SendConfirmationConsentEmailAsync_Calls_NotificationClient_SendEmailAsync_Owner_Throws()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentOwnerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerConfirmEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendConsentConfirmRequest.OwnerEmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentOwnerConfirmEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null)).Throws<Exception>();

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendConsentConfirmRequest.InstallerEmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentInstallerConfirmEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null));

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendConsentConfirmationEmailAsync(_sendConsentConfirmRequest);

            // assert
            testResult.Should().BeEquivalentTo(new SendConsentConfirmationEmailResult { IsSuccess = false });
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.OwnerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(1));
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendConsentConfirmRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Exactly(0));
        }

        #endregion

        #region SendInstallerRejectionEmailAsync Tests

        [Test]
        public async Task SendRejectionEmailToInstallersNotChosenAsync_Calls_NotificationClient_SendEmailAsync_Throws()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerNotChosenEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _mockGovNotifyClient.Setup(x => x.SendEmailAsync(
                _sendInstallersNotChosenEmailRequest.InstallerEmailAddress,
                _mockOwnerConsentServiceOptions.Object.ConsentInstallerNotChosenEmailTemplateID,
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null)).Throws<Exception>();

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendRejectionEmailToInstallersNotChosenAsync(_sendInstallersNotChosenEmailRequest);

            // can't validate the dictionary because the token generated can vary for the same data, hence It.IsAny<Dictionary<string, dynamic>>()
            // assert
            testResult.Should().BeEquivalentTo(new SendInstallersNotChosenEmailResult() { IsSuccess = false });
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendInstallersNotChosenEmailRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Once);
        }

        [Test]
        public async Task SendRejectionEmailToInstallersNotChosenAsync_SendEmailAsync_Once_Given_A_Valid_Request()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerNotChosenEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var testResult = await _systemUnderTest.SendRejectionEmailToInstallersNotChosenAsync(_sendRejectionEmailRequest);

            //assert
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendRejectionEmailRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Once);
        }

        [Test]
        public async Task SendRejectionEmailToInstallersNotChosenAsync_SendEmailAsync_Once_Given_A_Invalid_Request()
        {
            // arrange
            _mockGovNotifyClient = new Mock<IAsyncNotificationClient>();
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentInstallerNotChosenEmailTemplateID).Returns(_fakeGovNotifyTemplateId);

            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            // act
            var result = await _systemUnderTest.SendRejectionEmailToInstallersNotChosenAsync(_sendInstallersNotChosenEmailRequest);

            // assert
            _mockGovNotifyClient.Verify(mock => mock.SendEmailAsync(_sendRejectionEmailRequest.InstallerEmailAddress, _fakeGovNotifyTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null), Times.Never);
        }
        #endregion

        #region VerifyToken Tests

        [Test]
        public async Task VerifyToken_Function_Accepts_Valid_Token()
        {
            //Arrange
            _token = GenerateToken(_sendConsentEmailRequest, DateTime.Parse(_tokenExpiryDate));
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            //Act
            var result = _systemUnderTest.VerifyToken(_token);

            //Assert
            result.TokenAccepted.Should().BeTrue();
        }

        [Test]
        public async Task VerifyToken_Function_Rejects_Invalid_Token()
        {
            //Arrange
            _mockOwnerConsentServiceOptions = new Mock<IOwnerConsentServiceOptions>();
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentTokenSecret).Returns(_fakeGovNotifyConsentTokenSecret);
            _mockOwnerConsentServiceOptions.Setup(m => m.ConsentEmailTemplateID).Returns(_fakeGovNotifyTemplateId);
            _systemUnderTest = new OwnerConsentService(_mockOwnerConsentServiceOptions.Object, _mockGovNotifyClient.Object);

            //Act
            var result = _systemUnderTest.VerifyToken(_invalidToken);

            //Assert
            result.TokenAccepted.Should().BeFalse();
        }

        #endregion
          
        #region test helpers
        private string GenerateToken(SendConsentEmailRequest request, DateTime tokenExpiryDate)
        {
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_fakeGovNotifyConsentTokenSecret));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("ConsentRequestId", request.ConsentRequestId.ToString()),
                    new Claim("ConsentRequestExpiryDate", tokenExpiryDate.ToString())
                }),
                Expires = DateTime.Parse(_tokenExpiryDate),
                SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #endregion


    }
}