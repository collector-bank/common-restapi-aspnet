// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestValidationFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.Library.Validation;
    using Collector.Common.RestContracts;

    public class RequestValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var validationError = GetValidationError(actionContext);

            if (validationError == null)
                return;

            var error = GetParseError(actionContext) ?? validationError;

            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, error);
        }

        private static Error GetParseError(HttpActionContext actionContext)
        {
            var firstParseException = actionContext.ModelState.Values
                                                   .SelectMany(v => v.Errors)
                                                   .FirstOrDefault(error => error.Exception != null)?.Exception;

            if (firstParseException == null)
                return null;

            return new Error(
                       code: "VALIDATION_ERROR",
                       message: firstParseException.Message);
        }

        private static Error GetValidationError(HttpActionContext actionContext)
        {
            var firstException = actionContext.ActionArguments.Values.Select(Validate).FirstOrDefault(e => e != null);

            if (firstException == null)
                return null;

            return new Error(
                       code: firstException.Message,
                       message: firstException.Message);
        }

        private static Exception Validate(object argument)
        {
            return argument == null
                ? new Exception("NULL_REQUEST")
                : AnnotationValidator.Validate(argument, AnnotationValidator.ValidationBehaviour.Deep);
        }
    }
}
