// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseLoggingFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System;
    using System.Web.Http.Filters;

    using Newtonsoft.Json;

    using Serilog;

    public class ResponseLoggingFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public ResponseLoggingFilter(ILogger logger)
        {
            _logger = logger.ForContext(GetType());
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            dynamic dynamicResponseContent = actionExecutedContext.ActionContext.Response.Content;
            var responseContent = actionExecutedContext.ActionContext.Response.Content;
            var mediaType = responseContent?.Headers?.ContentType?.MediaType;
            var statusCode = (int)actionExecutedContext.Response.StatusCode;

            var logger = _logger.ForContext("StatusCode", statusCode);

            if (!string.IsNullOrEmpty(mediaType))
            {
                logger = logger.ForContext("MediaType", mediaType);

                if (mediaType.ToLower().Contains("application/json"))
                    logger = logger.ForContext("ResponseBody", Serialize(dynamicResponseContent.Value));
            }

            var requestRecievedTime = actionExecutedContext.Request?.Properties?[RequestLoggingFilter.REQUEST_RECIEVED_TIME] as DateTimeOffset?;
            if (requestRecievedTime.HasValue)
                logger = logger.ForContext("ResponseTimeMilliseconds", (int)(DateTimeOffset.UtcNow - requestRecievedTime.Value).TotalMilliseconds);

            logger.Information("Rest response sent");
        }

        private static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }
    }
}
