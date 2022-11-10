using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.PropertyConsents.API.Controllers;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Ofgem.Lib.BUS.APIClient.Domain.Exceptions;

namespace Ofgem.API.BUS.PropertyConsents.WebApp.UnitTests;

public class OwnerConsentControllerTests
{
    private OwnerConsentController _ownerConsentControllerController;

    private readonly Mock<IOwnerConsentService> _ownerConsentServiceMock = new();
    private readonly Mock<IApplicationsAPIService> _applicationsApiServiceMock = new();

    [SetUp]
    public void Setup()
    {
        var owernConsentServiceMock = _ownerConsentServiceMock.Object;
        var applicationsServiceMock = _applicationsApiServiceMock.Object;

        _ownerConsentControllerController = new OwnerConsentController(owernConsentServiceMock, applicationsServiceMock);
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_First_Parameter_Are_Null()
    {
        // act assert
        Assert.That(
            () =>
            {
                _ownerConsentControllerController = new OwnerConsentController(null, _applicationsApiServiceMock.Object);
            },
            Throws.InstanceOf<ArgumentNullException>()
            .With.Property("ParamName").EqualTo("ownerConsentService"));
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_Second_Parameter_Are_Null()
    {
        // act assert
        Assert.That(
            () =>
            {
                _ownerConsentControllerController = new OwnerConsentController(_ownerConsentServiceMock.Object, null);
            },
            Throws.InstanceOf<ArgumentNullException>()
            .With.Property("ParamName").EqualTo("applicationsService"));
    }
    
    [Test]
    public async Task Send_Confirmation_Consent_Email_Successfully()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var mockOfConsentRequestSummary = new Mock<ConsentRequestSummary>();
        var mockOfSendConsentConfirmationEmailResult = new Mock<SendConsentConfirmationEmailResult>();
        var mockOfSendConsentConfirmationEmailRequest = new Mock<SendConsentConfirmationEmailRequest>();

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(It.IsAny<Guid>())).ReturnsAsync(mockOfConsentRequestSummary.Object);
        _ownerConsentServiceMock.Setup(x => x.SendConsentConfirmationEmailAsync(mockOfSendConsentConfirmationEmailRequest.Object)).ReturnsAsync(mockOfSendConsentConfirmationEmailResult.Object);

        //Act
        var result = await _ownerConsentControllerController.SendConfirmationConsentEmail(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Send_Confirmation_Consent_Email_UnSuccessfully_ModelState()
    {
        //Arrange
        _ownerConsentControllerController.ModelState.AddModelError("Error", "Error");
        var expectedResult = _ownerConsentControllerController.BadRequest(_ownerConsentControllerController.ModelState);

        //Act
        var result = await _ownerConsentControllerController.SendConfirmationConsentEmail(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Send_Confirmation_Consent_Email_UnSuccessfully_ThrowsBadRequestException()
    {
        //Arrange
        var consentRequestId = Guid.NewGuid();
        var ex = new BadRequestException(nameof(ConsentRequestSummary), HttpStatusCode.BadRequest);

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(consentRequestId)).Throws(ex);

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.SendConfirmationConsentEmail(consentRequestId);
        var result = await _ownerConsentControllerController.SendConfirmationConsentEmail(consentRequestId).ConfigureAwait(false);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
        act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Test]
    public async Task Send_Consent_Email_Successfully()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var mockOfSendConsentEmailRequest = new Mock<SendConsentEmailRequest>();
        var mockOfSendConsentEmailResult = new Mock<SendConsentEmailResult>();

        _ownerConsentServiceMock.Setup(x => x.SendConsentEmailAsync(mockOfSendConsentEmailRequest.Object)).ReturnsAsync(mockOfSendConsentEmailResult.Object);

        //Act
        var result = await _ownerConsentControllerController.SendConsentEmail(mockOfSendConsentEmailRequest.Object).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void Send_Consent_Email_UnSuccessfully()
    {
        //Arrange
        var mockOfSendConsentEmailRequest = new Mock<SendConsentEmailRequest>();

        _ownerConsentServiceMock.Setup(x => x.SendConsentEmailAsync(mockOfSendConsentEmailRequest.Object)).Throws(new Exception(nameof(SendConsentEmailResult)));

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.SendConsentEmail(mockOfSendConsentEmailRequest.Object);

        //Assert
        act.Should().ThrowExactlyAsync<Exception>();
    }

    [Test]
    public async Task Send_Consent_Email_UnSuccessfully_ModelState()
    {
        //Arrange
        _ownerConsentControllerController.ModelState.AddModelError("Error", "Error");
        var expectedResult = _ownerConsentControllerController.BadRequest(_ownerConsentControllerController.ModelState);

        //Act
        var result = await _ownerConsentControllerController.SendConsentEmail(new SendConsentEmailRequest()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Send_Consent_Email_UnSuccessfully_ThrowsBadRequestException()
    {
        //Arrange
        var mockOfSendConsentEmailRequest = new Mock<SendConsentEmailRequest>();
        var ex = new BadRequestException(nameof(SendConsentEmailResult), HttpStatusCode.BadRequest);

        _ownerConsentServiceMock.Setup(x => x.SendConsentEmailAsync(mockOfSendConsentEmailRequest.Object)).Throws(ex);

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.SendConsentEmail(mockOfSendConsentEmailRequest.Object);
        var result = await _ownerConsentControllerController.SendConsentEmail(mockOfSendConsentEmailRequest.Object).ConfigureAwait(false);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
        act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Test]
    public void VerifyToken_Successfully()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var mockOfTokenVerificationResult = new Mock<TokenVerificationResult>();

        _ownerConsentServiceMock.Setup(x => x.VerifyToken(It.IsAny<string>())).Returns(mockOfTokenVerificationResult.Object);

        //Act
        var result = _ownerConsentControllerController.VerifyToken(It.IsAny<string>());

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void VerifyToken_UnSuccessfully_When_Parameter_Is_Empty()
    {
        //Arrange
        _ownerConsentServiceMock.Setup(x => x.VerifyToken(null)).Throws(new ArgumentNullException(nameof(TokenVerificationResult)));

        //Act
        Func<IActionResult> act = () => _ = _ownerConsentControllerController.VerifyToken(null);

        //Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'TokenVerificationResult')");
    }

    [Test]
    public void VerifyToken_UnSuccessfully_ThrowsBadRequestException()
    {
        //Arrange
        var token = "Token";
        var ex = new BadRequestException(nameof(SendConsentEmailResult), HttpStatusCode.BadRequest);

        _ownerConsentServiceMock.Setup(x => x.VerifyToken(token)).Throws(ex);

        //Act
        var result = _ownerConsentControllerController.VerifyToken(token);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Get_Consent_Request_Summary_Successfully()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();
        var mockOfConsentRequestSummary = new Mock<ConsentRequestSummary>();

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(It.IsAny<Guid>())).ReturnsAsync(mockOfConsentRequestSummary.Object);

        //Act
        var result = await _ownerConsentControllerController.GetConsentRequestSummary(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void Get_Consent_Request_Summary_UnSuccessfully()
    {
        //Arrange
        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(It.IsAny<Guid>())).Throws(new InvalidOperationException(nameof(ConsentRequestSummary)));

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.GetConsentRequestSummary(It.IsAny<Guid>());

        //Assert
        act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("API call succeeded, but returned a non-success result");
    }

    [Test]
    public async Task Get_Consent_Request_Summary_UnSuccessfully_ThrowsBadRequestException()
    {
        //Arrange
        var consentRequestId = Guid.NewGuid();
        var ex = new BadRequestException(nameof(ConsentRequestSummary), HttpStatusCode.BadRequest);

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(consentRequestId)).Throws(ex);

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.GetConsentRequestSummary(consentRequestId);
        var result = await _ownerConsentControllerController.GetConsentRequestSummary(consentRequestId).ConfigureAwait(false);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
        act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Test]
    public async Task Register_Owner_Consent_Received_IsSuccess()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var returnResult = new RegisterOwnerConsentResult { IsIneligible = false, IsSuccess = true };

        _applicationsApiServiceMock.Setup(x => x.RegisterConsentAsync(It.IsAny<Guid>())).ReturnsAsync(returnResult);

        //Act
        var result = await _ownerConsentControllerController.RegisterOwnerConsentReceived(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        var isSuccess = ((RegisterOwnerConsentResult)((ObjectResult)result).Value).IsSuccess;
        isSuccess.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Register_Owner_Consent_Received_IsIneligible()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var returnResult = new RegisterOwnerConsentResult { IsIneligible = true, IsSuccess = false };

        _applicationsApiServiceMock.Setup(x => x.RegisterConsentAsync(It.IsAny<Guid>())).ReturnsAsync(returnResult);

        //Act
        var result = await _ownerConsentControllerController.RegisterOwnerConsentReceived(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        var IsIneligible = ((RegisterOwnerConsentResult)((ObjectResult)result).Value).IsIneligible;
        IsIneligible.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Register_Owner_Consent_Received_ThrowsBadRequestException()
    {
        //Arrange
        var consentRequestId = Guid.NewGuid();
        var ex = new BadRequestException(nameof(RegisterOwnerConsentResult), HttpStatusCode.BadRequest);

        _applicationsApiServiceMock.Setup(x => x.RegisterConsentAsync(consentRequestId)).Throws(ex);

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.RegisterOwnerConsentReceived(consentRequestId);
        var result = await _ownerConsentControllerController.RegisterOwnerConsentReceived(consentRequestId).ConfigureAwait(false);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
        act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Test]
    public async Task StoreFeedback_Success_True()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok(new StoreFeedBackResult { IsSuccess = true });

        _applicationsApiServiceMock.Setup(x => x.StoreFeedback(It.IsAny<StoreFeedBackRequest>())).ReturnsAsync(true);

        //Act
        var result = await _ownerConsentControllerController.StoreFeedback(It.IsAny<StoreFeedBackRequest>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Send_Application_Rejection_Email_To_Installers_NotChosen_UnSuccessfully_Throws_Bad()
    {
        //Arrange
        var consentRequestId = Guid.NewGuid();
        var ex = new BadRequestException(nameof(ConsentRequestSummary), HttpStatusCode.BadRequest);

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(consentRequestId)).Throws(ex);

        //Act
        Func<Task<IActionResult>>? act = () => _ = _ownerConsentControllerController.SendApplicationRejectionEmailToinstallersNotChosen(consentRequestId);
        var result = await _ownerConsentControllerController.SendApplicationRejectionEmailToinstallersNotChosen(consentRequestId).ConfigureAwait(false);

        //Assert
        var expectedResult = _ownerConsentControllerController.BadRequest();
        result.Should().BeEquivalentTo(expectedResult);
        act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Test]
    public async Task Send_Application_Rejection_Email_To_installers_No_Rejected_Applications()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var applications = new List<Application>();

        var mockOfConsentRequestSummary = new Mock<ConsentRequestSummary>();
        var mockOfSendInstallersNotChosenEmailRequest = new Mock<SendInstallersNotChosenEmailRequest>();
        var mockOFSendInstallersNotChosenEmailResult = new Mock<SendInstallersNotChosenEmailResult>();

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(It.IsAny<Guid>())).ReturnsAsync(mockOfConsentRequestSummary.Object);
        _applicationsApiServiceMock.Setup(x => x.GetAssociatedApplications(It.IsAny<Guid>())).ReturnsAsync(applications);
        _applicationsApiServiceMock.Setup(x => x.GetBusinessAccountEmailByInstallerId(It.IsAny<Guid>())).ReturnsAsync(It.IsAny<string>());

        _ownerConsentServiceMock.Setup(x => x.SendRejectionEmailToInstallersNotChosenAsync(mockOfSendInstallersNotChosenEmailRequest.Object)).ReturnsAsync(mockOFSendInstallersNotChosenEmailResult.Object);

        //Act
        var result = await _ownerConsentControllerController.SendApplicationRejectionEmailToinstallersNotChosen(It.IsAny<Guid>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task Send_Application_Rejection_Email_To_installers_Has_Rejected_Applications()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.Ok();

        var consentRequestId = Guid.NewGuid();
        
        var installerIdNotChosen = new Guid("8e8fa3f3-8e5a-4c54-893a-70d6ef1b0ec6");
        var installerId = new Guid("9e8fa3f3-8e5a-4c54-893a-70d6ef1b0ec6");
        var installerEmail = "avinash.shinde@ofgem.gov.uk";
        var applications = new List<Application>
        {
            new Application
            {
                SubmitterId = installerId
            },
            new Application
            {
                SubmitterId = installerIdNotChosen
            }
        };

        var consentRequestSummary = new ConsentRequestSummary
        {
            ApplicationReferenceNumber = "APW12345678",
            InstallerName = "APW Enterprises",
            OwnerEmailId = "avinash.shinde@ofgem.gov.uk",
            InstallerEmailId = installerEmail,
            TechnologyType = "Air source heat pump",
            InstallationAddressLine1 = "1 Canada Square",
            InstallationAddressLine2 = "Canary Wharf",
            InstallationAddressLine3 = "London",
            InstallationAddressCounty = "",
            InstallationAddressPostcode = "E14 5AA",
            InstallationAddressUprn = "123456123456",
            ServiceLevelAgreementDate = DateTime.Now.AddYears(1),
            QuoteAmount = 11405
        };

        _applicationsApiServiceMock.Setup(x => x.GetConsentRequestSummaryAsync(consentRequestId)).ReturnsAsync(consentRequestSummary);
        _applicationsApiServiceMock.Setup(x => x.GetAssociatedApplications(consentRequestId)).ReturnsAsync(applications);
        _applicationsApiServiceMock.Setup(x => x.GetBusinessAccountEmailByInstallerId(installerId)).ReturnsAsync(installerEmail);
        _applicationsApiServiceMock.Setup(x => x.GetBusinessAccountEmailByInstallerId(installerIdNotChosen)).ReturnsAsync("Non-email");

        _ownerConsentServiceMock.Setup(x => x.SendRejectionEmailToInstallersNotChosenAsync(It.IsAny<SendInstallersNotChosenEmailRequest>())).ReturnsAsync(new SendInstallersNotChosenEmailResult());

        //Act
        var result = await _ownerConsentControllerController.SendApplicationRejectionEmailToinstallersNotChosen(consentRequestId).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
        _ownerConsentServiceMock.Verify(x => x.SendRejectionEmailToInstallersNotChosenAsync(It.IsAny<SendInstallersNotChosenEmailRequest>()), Times.Once);
    }

    [Test]
    public async Task StoreFeedback_Success_False()
    {
        //Arrange
        var expectedResult = _ownerConsentControllerController.BadRequest(new StoreFeedBackResult { IsSuccess = false });

        _applicationsApiServiceMock.Setup(x => x.StoreFeedback(It.IsAny<StoreFeedBackRequest>())).ThrowsAsync(new Exception());

        //Act
        var result = await _ownerConsentControllerController.StoreFeedback(It.IsAny<StoreFeedBackRequest>()).ConfigureAwait(false);

        //Assert
        result.Should().BeEquivalentTo(expectedResult);
    }
}
