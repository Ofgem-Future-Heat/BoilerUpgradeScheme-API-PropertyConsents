using Microsoft.AspNetCore.Mvc;
using Ofgem.Lib.BUS.APIClient.Domain.Exceptions;
using Ofgem.Lib.BUS.APIClient.Domain.Models;
using System.Net;

namespace Ofgem.API.BUS.PropertyConsents.API.Extensions
{
    /// <summary>
    /// Controller extensions
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Bad request object result.
        /// </summary>
        /// <param name="controllerBase">The controller.</param>
        /// <param name="ex">BadRequestException.</param>
        /// <returns></returns>
        public static ActionResult AsObjectResult(this ControllerBase controllerBase, BadRequestException ex)
        {
            var request = FormatRequest(ex);

            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return controllerBase.NotFound(request);
            }
            else if (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return controllerBase.NoContent();
            }

            return controllerBase.BadRequest(request);
        }

        private static object FormatRequest(BadRequestException ex)
        {
            if (ex.Errors != null && ex.Errors.Any())
            {
                return new RequestMessage { Title = ex.Message, Status = ex.StatusCode, Errors = ex.Errors };
            }
            return new RequestMessage { Title = ex.Message, Status = ex.StatusCode };
        }
    }
}
