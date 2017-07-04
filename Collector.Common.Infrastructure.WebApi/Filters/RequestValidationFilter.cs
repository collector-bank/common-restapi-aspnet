// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestValidationFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

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
            
            if(!errors.Any())
                errors = GetContractValidationErrors(actionContext);

            if (!errors.Any())
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

        private static IList<ErrorInfo> GetParseErrors(HttpActionContext actionContext)
        {
            return actionContext.ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Where(error => error.Exception != null)
                                .Select(error => error.Exception)
                                .Select(e => new ErrorInfo(e.Message, "PARSE_ERROR"))
                                .ToList();
        }

        private static IList<ErrorInfo> GetContractValidationErrors(HttpActionContext actionContext)
        {
            return actionContext.ActionArguments.Values
                                .OfType<IRequest>()
                                .SelectMany(request => request.GetValidationErrors())
                                .ToList();
        }
    }
}
