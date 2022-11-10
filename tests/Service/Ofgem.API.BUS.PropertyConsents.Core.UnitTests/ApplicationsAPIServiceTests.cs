using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.Applications.Client.Interfaces;
using Ofgem.API.BUS.Applications.Domain;
using Ofgem.API.BUS.Applications.Domain.Constants;
using Ofgem.API.BUS.Applications.Domain.Entities.CommsObjects;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.API.BUS.PropertyConsents.Domain.Models.CommsObjects;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ofgem.API.BUS.Applications.Domain.ApplicationSubStatus;

namespace Ofgem.API.BUS.PropertyConsents.Core.UnitTests;

[TestFixture]
public class ApplicationsAPIServiceTests : BaseServiceTests
{
    private ApplicationsAPIService _systemUnderTest;
    private Mock<IApplicationsAPIClient> _mockApplicationsAPIClient = new();
    private Mock<IConsentRequestsRequestsClient> _mockConsentRequestsRequestsClient = new();
    private Mock<IOwnerConsentService> _mockOwnerConsentService = new();
    private readonly Guid _fakeConsentRequestId = Guid.NewGuid();

    private readonly GetConsentRequestDetailsResult _aValidGetConsentRequestDetailsAPIResponse = new GetConsentRequestDetailsResult()
    {
        IsSuccess = true,
        ConsentRequestSummary = new()
        {
            ApplicationReferenceNumber = "APW12345678",
            InstallerName = "APW Enterprises",
            OwnerEmailId = "avinash.shinde@ofgem.gov.uk",
            InstallerEmailId = "avinash.shinde@ofgem.gov.uk",
            TechnologyType = "Air source heat pump",
            InstallationAddressLine1 = "1 Canada Square",
            InstallationAddressLine2 = "Canary Wharf",
            InstallationAddressLine3 = "London",
            InstallationAddressCounty = "",
            InstallationAddressPostcode = "E14 5AA",
            InstallationAddressUprn = "123456123456",
            ServiceLevelAgreementDate = DateTime.Now.AddYears(1),
            QuoteAmount = 11405
        }
    };

    #region Constructor Tests

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_ApplicationsApiClient_Is_Null()
    {
        // act assert
        Action act = () => new ApplicationsAPIService(null!, _mockOwnerConsentService.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("applicationsAPIClient");
    }

    [Test]
    public void Constructor_Throws_ArgumentNullException_If_OwnerConsentService_Is_Null()
    {
        // act assert
        Action act = () => new ApplicationsAPIService(_mockApplicationsAPIClient.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("ownerConsentService");
    }

    [Test]
    public void Can_Be_Instantiated_With_Valid_Parameters()
    {
        // act
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // assert
        Assert.IsTrue(_systemUnderTest != null);
    }

    #endregion

    #region GetConsentRequestSummaryAsync Tests

    [Test]
    public async Task GetConsentRequestSummaryAsync_Calls_ApplicationsAPIClient_ConsentRequestsRequestsClient_GetDetailsAsync_Once()
    {
        // arrange
        _mockConsentRequestsRequestsClient = new Mock<IConsentRequestsRequestsClient>();
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);
        _mockApplicationsAPIClient = new Mock<IApplicationsAPIClient>();
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        _ = await _systemUnderTest.GetConsentRequestSummaryAsync(_fakeConsentRequestId);

        // assert
        _mockConsentRequestsRequestsClient.Verify(mock => mock.GetDetailsAsync(_fakeConsentRequestId), Times.Once);
    }

    [Test]
    public void GetConsentRequestSummaryAsync_Calls_Throws_Null_Reference_Exception()
    {
        // arrange
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient.GetDetailsAsync(_fakeConsentRequestId)).ThrowsAsync(new NullReferenceException());
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act & assert
        Assert.That(() => _systemUnderTest.GetConsentRequestSummaryAsync(_fakeConsentRequestId), Throws.TypeOf<System.NullReferenceException>());
    }

    [Test]
    public void GetConsentRequestSummaryAsync_Calls_Throws_InvalidOperation_Exception_NonSuccess()
    {
        // arrange
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient.GetDetailsAsync(_fakeConsentRequestId))
            .ReturnsAsync(new GetConsentRequestDetailsResult
            {
                IsSuccess = false,
                ConsentRequestSummary = null
            });

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        //// act & assert
        Assert.That(() => _systemUnderTest.GetConsentRequestSummaryAsync(_fakeConsentRequestId), Throws.TypeOf<System.InvalidOperationException>(), "API call succeeded, but returned a non-success result");
    }

    [Test]
    public void GetConsentRequestSummaryAsync_Calls_Throws_InvalidOperation_Exception_NullSummary()
    {
        // arrange
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient.GetDetailsAsync(_fakeConsentRequestId))
            .ReturnsAsync(new GetConsentRequestDetailsResult
            {
                IsSuccess = true,
                ConsentRequestSummary = null
            });

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        //// act & assert
        Assert.That(() => _systemUnderTest.GetConsentRequestSummaryAsync(_fakeConsentRequestId), Throws.TypeOf<System.InvalidOperationException>(), "API call succeeded, but ConsentRequestSummary was null");
    }

    #endregion

    #region GetBusinessAccountEmailByInstallerId Tests

    [Test]
    public async Task GetBusinessAccountEmailByInstallerId_Return_Email()
    {
        // arrange
        var installerId = Guid.NewGuid();
        var email = "email@emailUnitTest.com";
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient.GetBusinessAccountEmailByInstallerId(installerId)).ReturnsAsync(email);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        //// act & assert
        var result = await _systemUnderTest.GetBusinessAccountEmailByInstallerId(installerId);
        result.Should().Be(email);
    }

    #endregion

    #region RegisterConsentAsync Tests
    [Test]
    public async Task RegisterConsentAsync_Calls_ApplicationsAPIClient_ConsentRequestsRequestsClient_RegisterConsentReceivedAsync_Once()
    {
        // arrange
    
        _mockConsentRequestsRequestsClient.Setup(m => m.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>())).Verifiable();
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);

        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest { ID  = _fakeConsentRequestId });

        var applications = new List<Application> { application };

        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.GetApplicationsByUprnAsync(It.IsAny<string>()).Result).Returns(applications);

        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        await _systemUnderTest.RegisterConsentAsync(_fakeConsentRequestId);

        // assert
        _mockConsentRequestsRequestsClient.Verify(mock => mock.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>()), Times.Once());
    }

    [Test]
    public void RegisterConsentAsync_Throws_Argument_Null_Excepion()
    {
        // arrange
    
        _mockConsentRequestsRequestsClient.Setup(m => m.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>()))
            .ThrowsAsync(new ArgumentNullException());

        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ThrowsAsync(new ArgumentNullException());

        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application };

        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.GetApplicationsByUprnAsync(It.IsAny<string>()).Result).Returns(applications);

        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        Assert.That(() => _systemUnderTest.RegisterConsentAsync(_fakeConsentRequestId), Throws.TypeOf<System.ArgumentNullException>());
    }

    [Test]
    public async Task RegisterConsentAsync_Calls_ApplicationsAPIClient_ConsentRequestsRequestsClient_Returns_Ineligible()
    {
        // arrange

        _mockConsentRequestsRequestsClient.Setup(m => m.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>())).Verifiable();
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);

        // Arrange
        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest { ConsentReceivedDate = DateTime.Now });
        application.SubStatus.Code = ApplicationSubStatus.ApplicationSubStatusCode.INRW;

        var applications = new List<Application> { application };

        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.GetApplicationsByUprnAsync(It.IsAny<string>()).Result).Returns(applications);

        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        var result = await _systemUnderTest.RegisterConsentAsync(_fakeConsentRequestId);

        // assert
        result.IsIneligible.Should().BeTrue();
        _mockConsentRequestsRequestsClient.Verify(mock => mock.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>()), Times.Once());
    }


    #endregion

    #region IsConsentRequestEligible Tests

    [Test]
    public void IsApplicationEligible_Throws_ArgumentNullException_Null_Applications()
    {
        // Arrange
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act Assert
        Assert.That(() => _systemUnderTest.IsApplicationEligible(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void IsApplicationEligible_Throws_ArgumentNullException_Empty_Applications()
    {
        // Arrange
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act Assert
        Assert.That(() => _systemUnderTest.IsApplicationEligible(new List<Application>()), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void IsConsentRequestEligible_Returns_True_For_Eligible_Application_With_Single_Eligible_Consent_Request()
    {
        // Arrange
        // Given a single application associated with a UPRN, and that the property owner has not yet given consent...
        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        // ... when we check for eligibility on the consent request...
        var isEligible = _systemUnderTest.IsApplicationEligible(applications);

        // Assert
        // ... they are eligble to consent to the application
        Assert.IsTrue(isEligible);
    }

    [Test]
    public void IsConsentRequestEligible_Returns_True_For_Eligible_Application_With_Multiple_Eligible_Consent_Requests()
    {
        // Arrange
        // Given multiple applications associated with a UPRN, and that the property owner has not yet given consent to any of them...
        var application1 = GenerateApplication();
        application1.ConsentRequests.Add(new ConsentRequest());

        var application2 = GenerateApplication();
        application2.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application1, application2 };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        // ... when we check for eligibility on a particular consent request
        var isEligible = _systemUnderTest.IsApplicationEligible(applications);

        // Assert
        // ... they are eligible to consent to the application
        Assert.IsTrue(isEligible);
    }

    [Test]
    public void IsConsentRequestEligible_Returns_True_For_Eligible_Application_With_Multiple_Rejected_Consent_Requests()
    {
        // Arrange
        // Given multiple applications associated with a UPRN, and that previous applications have been rejected...
        var application1 = GenerateApplication();
        application1.ConsentRequests.Add(new ConsentRequest { ConsentReceivedDate = DateTime.Now });
        application1.SubStatus.Code = ApplicationSubStatus.ApplicationSubStatusCode.CNTRD;

        var application2 = GenerateApplication();
        application2.ConsentRequests.Add(new ConsentRequest { ConsentReceivedDate = DateTime.Now });
        application2.SubStatus.Code = ApplicationSubStatus.ApplicationSubStatusCode.REJECTED;

        var application3 = GenerateApplication();
        application3.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application1, application2, application3 };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        // ... when we check for eligibility on a particular consent request
        var isEligible = _systemUnderTest.IsApplicationEligible(applications);

        // Assert
        // ... they are eligible to consent to the application
        Assert.IsTrue(isEligible);
    }

    [Test]
    public void IsConsentRequestEligible_Returns_False_For_Ineligible_Application_With_Single_Consent_Request()
    {
        // Arrange
        // Given a single application associated with a UPRN, and that the property owner has already given consent...
        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest { ConsentReceivedDate = DateTime.Now });
        application.SubStatus.Code = ApplicationSubStatus.ApplicationSubStatusCode.INRW;

        var applications = new List<Application> { application };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        // ... when we check for eligibility on the consent request...
        var isEligible = _systemUnderTest.IsApplicationEligible(applications);

        // Assert
        // ... they are ineligible and cannot consent again.
        Assert.IsFalse(isEligible);
    }

    [Test]
    public void IsConsentRequestEligible_Returns_False_For_Ineligible_Application_With_Multiple_Consent_Requests()
    {
        // Arrange
        // Given multiple applications associated with a UPRN, and that the property owner has already consented to one of them...

        var application1 = GenerateApplication();
        application1.ConsentRequests.Add(new ConsentRequest { ConsentReceivedDate = DateTime.Now });
        application1.SubStatus.Code = ApplicationSubStatus.ApplicationSubStatusCode.INRW;

        var application2 = GenerateApplication();
        application2.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application1, application2 };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        // ... when we check for eligibility on the consent request...
        var isEligible = _systemUnderTest.IsApplicationEligible(applications);

        // Assert
        // ... they are ineligible to consent to the application
        Assert.IsFalse(isEligible);
    }

    #endregion
    #region GetAssociatedApplications Tests
    [Test]
    public async Task GetAssociatedApplications_Returns_Success()
    {
        // arrange
    
        _mockConsentRequestsRequestsClient.Setup(m => m.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>())).Verifiable();
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);

        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application };
        applications.Add(new Application());

        // arrange
    
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);
        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.GetApplicationsByUprnAsync(It.IsAny<string>()).Result).Returns(applications);

        var _fakeUprn = "1234";

        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient.GetApplicationsByUprnAsync(_fakeUprn)).ReturnsAsync(applications);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        var listApplications = await _systemUnderTest.GetAssociatedApplications(_fakeConsentRequestId);

        // assert
        Assert.That(listApplications.Count() > 0);
    }

    [Test]
    public void GetAssociatedApplications_Returns_Exception()
    {
        // arrange
    
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId))
            .ThrowsAsync(new ArgumentNullException());

        // arrange
        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        Assert.That(() => _systemUnderTest.GetAssociatedApplications(_fakeConsentRequestId), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion
    #region GetApplicationsByUprnAsync Tests
    [Test]
    public async Task GetApplicationsByUprnAsync_Returns_Success()
    {
        // arrange
        var _fakeUprn = "1234";
        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application };
        applications.Add(application);

        // arrange
    
        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();

        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient.GetApplicationsByUprnAsync(_fakeUprn)).ReturnsAsync(applications);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        var listApplications = await _systemUnderTest.GetApplicationsByUprnAsync(_fakeUprn);

        // assert
        Assert.That(listApplications.Count() > 0);
    }

    [Test]
    public void GetApplicationsByUprnAsync_Returns_Exception()
    {
        // arrange
    
        var _fakeUprn = "1234";
        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest());

        var applications = new List<Application> { application };
        applications.Add(application);

        // arrange
    
        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();


        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient.GetApplicationsByUprnAsync(_fakeUprn)).ThrowsAsync(new ArgumentNullException());

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act & assert
        Assert.That(() => _systemUnderTest.GetApplicationsByUprnAsync(_fakeUprn), Throws.TypeOf<ArgumentNullException>());
    }
    #endregion

    #region HandleCompetingApplications Tests
    [Test]
    public async Task HandleCompetingApplications_Returns_Success()
    {
        // Arrange
        var subApplication = GenerateApplication();
        subApplication.ConsentRequests.Add(new ConsentRequest { ID = Guid.NewGuid() });
        subApplication.SubStatus = new ApplicationSubStatus { Code = ApplicationSubStatusCode.SUB, Description = "Submitted", Id = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB] };
        subApplication.SubStatusId = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.SUB];

        var inReviewApplication = GenerateApplication();
        inReviewApplication.ConsentRequests.Add(new ConsentRequest { ID = Guid.NewGuid() });
        inReviewApplication.SubStatus = new ApplicationSubStatus { Code = ApplicationSubStatusCode.INRW, Description = "In Review", Id = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.INRW] };
        inReviewApplication.SubStatusId = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.INRW];

        var qcApplication = GenerateApplication();
        qcApplication.ConsentRequests.Add(new ConsentRequest { ID = Guid.NewGuid() });
        qcApplication.SubStatus = new ApplicationSubStatus { Code = ApplicationSubStatusCode.QC, Description = "QC", Id = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.QC] };
        qcApplication.SubStatusId = StatusMappings.ApplicationSubStatus[ApplicationSubStatusCode.QC];
        qcApplication.QCRecommendation = false;

        var fakeApplicationReturnList = new List<Application>
        {
            subApplication,
            inReviewApplication,
            qcApplication
        };

        _mockConsentRequestsRequestsClient = new Mock<IConsentRequestsRequestsClient>();
        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.UpdateApplicationStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AuditLogParameters>()))
                                      .ReturnsAsync(new List<string>());

        
        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // act
        var result = await _systemUnderTest.HandleCompetingApplications(fakeApplicationReturnList, new AuditLogParameters());

        // assert
        result.Should().BeTrue();
    }


    #endregion

    #region StoreFeedback Tests

    [Test]
    public async Task StoreFeedback_Success_False_Null_Request()
    {
        // Arrange
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        var feedbackResult = await _systemUnderTest.StoreFeedback(null!);

        // Assert
        feedbackResult.Should().BeFalse();
    }

    [Test]
    public async Task StoreFeedback_Success_False_Throws_Exception()
    {
        // Arrange
        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        var feedbackResult = await _systemUnderTest.StoreFeedback(new StoreFeedBackRequest());

        // Assert
        feedbackResult.Should().BeFalse();
    }

    [Test]
    public async Task StoreFeedback_Success()
    {
        // Arrange
        _mockConsentRequestsRequestsClient.Setup(m => m.RegisterConsentReceivedAsync(_fakeConsentRequestId, It.IsAny<RegisterConsentReceivedRequest>(), It.IsAny<AuditLogParameters>())).Verifiable();
        _mockConsentRequestsRequestsClient.Setup(m => m.GetDetailsAsync(_fakeConsentRequestId)).ReturnsAsync(_aValidGetConsentRequestDetailsAPIResponse);

        var application = GenerateApplication();
        application.ConsentRequests.Add(new ConsentRequest { ID = _fakeConsentRequestId });

        var applicationId = application.ID;

        var applications = new List<Application> { application };

        var mockApplicationsRequestsClient = new Mock<IApplicationsRequestsClient>();
        mockApplicationsRequestsClient.Setup(m => m.GetApplicationsByUprnAsync(It.IsAny<string>()).Result).Returns(applications);


        _mockApplicationsAPIClient.Setup(m => m.ConsentRequestsRequestsClient).Returns(_mockConsentRequestsRequestsClient.Object);
        _mockApplicationsAPIClient.Setup(m => m.ApplicationsRequestsClient).Returns(mockApplicationsRequestsClient.Object);

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);


        var feedbackRequest = new StoreFeedBackRequest
        {
            ConsentRequestId = _fakeConsentRequestId,
            FeedbackNarrative = "FeedbackNarrative",
            SurveyOption = 1
        };

        _systemUnderTest = new ApplicationsAPIService(_mockApplicationsAPIClient.Object, _mockOwnerConsentService.Object);

        // Act
        var feedbackResult = await _systemUnderTest.StoreFeedback(feedbackRequest);

        // Assert
        feedbackResult.Should().BeTrue();
        _mockApplicationsAPIClient.Verify(mock => mock.ApplicationsRequestsClient
            .StoreServiceFeedback(
                new Dictionary<string, string>
                {
                    { "ApplicationId", applicationId.ToString() },
                    { "FeedbackNarrative" , "FeedbackNarrative" },
                    { "SurveyOption" , "1" },
                    { "ServiceUsed" , "Consent" }
                },
                new AuditLogParameters
                {
                    EntityReferenceId = applicationId,
                    Username = "chester@tester.com",
                    UserType = "Consent"
                })
            , Times.Once);
    }

    #endregion
}
