using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ofgem.API.BUS.PropertyConsents.API.Controllers;
using Ofgem.API.BUS.PropertyConsents.API.Extensions;
using Ofgem.API.BUS.PropertyConsents.Core.Interfaces;
using Ofgem.Lib.BUS.APIClient.Domain.Exceptions;
using Ofgem.Lib.BUS.APIClient.Domain.Models;

namespace Ofgem.API.BUS.PropertyConsents.WebApp.UnitTests
{
    public class ExtensionsTests
    {
        [Test]
        public void AsObjectResult_Returns_NotFound()
        {
            var controller = new OwnerConsentController(Mock.Of<IOwnerConsentService>(), Mock.Of<IApplicationsAPIService>());
            var exception = new BadRequestException("message", HttpStatusCode.NotFound);

            var result = controller.AsObjectResult(exception);

            result.Should().BeEquivalentTo(controller.NotFound(new RequestMessage { Title = "message", Status = HttpStatusCode.NotFound }));
        }

        [Test]
        public void AsObjectResult_Returns_NotFound_WithEmptyErrorDictionary()
        {
            var controller = new OwnerConsentController(Mock.Of<IOwnerConsentService>(), Mock.Of<IApplicationsAPIService>());
            var exception = new BadRequestException(
                "message",
                new Dictionary<string, string[]>(),
                HttpStatusCode.NotFound);

            var result = controller.AsObjectResult(exception);

            result.Should().BeEquivalentTo(controller.NotFound(new RequestMessage { Title = "message", Status = HttpStatusCode.NotFound }));
        }

        [Test]
        public void AsObjectResult_Returns_NotFound_WithErrorDictionary()
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Error1", new[] { "Error Description" } }
            };

            var controller = new OwnerConsentController(Mock.Of<IOwnerConsentService>(), Mock.Of<IApplicationsAPIService>());
            var exception = new BadRequestException("message", errors, HttpStatusCode.NotFound);

            var result = controller.AsObjectResult(exception);

            result.Should().BeEquivalentTo(controller.NotFound(new RequestMessage { Title = "message", Errors = errors, Status = HttpStatusCode.NotFound })); }

        [Test]
        public void AsObjectResult_Returns_NoContent()
        {
            var controller = new OwnerConsentController(Mock.Of<IOwnerConsentService>(), Mock.Of<IApplicationsAPIService>());
            var exception = new BadRequestException("message", HttpStatusCode.NoContent);

            var result = controller.AsObjectResult(exception);

            result.Should().BeEquivalentTo(controller.NoContent());
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotAcceptable)]
        public void AsObjectResult_Returns_Expected(HttpStatusCode statusCode)
        {
            var controller = new OwnerConsentController(Mock.Of<IOwnerConsentService>(), Mock.Of<IApplicationsAPIService>());
            var exception = new BadRequestException("message", statusCode);

            var result = controller.AsObjectResult(exception);

            result.Should().BeEquivalentTo(controller.BadRequest(new RequestMessage { Title = "message", Status = statusCode }));
        }
    }
}
