// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestValidationFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.Library.Collections;
    using Collector.Common.Library.Collections.Interfaces;
    using Collector.Common.Library.Utils;
    using Collector.Common.Library.Validation;
    using Collector.Common.RestContracts;

    /// <summary>
    /// A filter for validating incoming requests using attribute validation.
    /// </summary>
    public class RequestValidationFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var errors = GetParseErrors(actionContext);
            
            if(errors.IsEmpty())
                errors = GetContractValidationErrors(actionContext);

            if (errors.IsEmpty())
                return;

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                new Response<object>
                    {
                        Error = new Error
                                    {
                                        Code = $"{(int)HttpStatusCode.BadRequest}",
                                        Message = HttpStatusCode.BadRequest.ToString(),
                                        Errors = errors
                                    }
                    });
        }

        private static IFixedEnumerable<ErrorInfo> GetParseErrors(HttpActionContext actionContext)
        {
            return actionContext.ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Where(error => error.Exception != null)
                                .Select(error => error.Exception)
                                .Select(e => new ErrorInfo(e.Message, "PARSE_ERROR"))
                                .ToFixed();
        }

        private static IFixedEnumerable<ErrorInfo> GetContractValidationErrors(HttpActionContext actionContext)
        {
            return actionContext.ActionArguments.Values
                                .SelectMany(ValidateActionArgument)
                                .Where(e => e != null)
                                .Select(e => new ErrorInfo(e.Message, "VALIDATION_ERROR"))
                                .ToFixed();
        }

        private static IEnumerable<Exception> ValidateActionArgument(object argument)
        {
            if (argument == null)
                return new[] { new ValidationException("NULL_REQUEST") };

            return AnnotationValidator.GetAllValidationErrors(argument, AnnotationValidator.ValidationBehaviour.Deep);
        }
    }
}
