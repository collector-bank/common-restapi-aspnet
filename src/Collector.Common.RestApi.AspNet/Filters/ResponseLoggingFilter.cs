namespace Collector.Common.RestApi.AspNet.Filters
{
    using System;
    using System.Linq;
    using System.Web.Http.Filters;

    using Collector.Common.RestContracts.Interfaces;

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
            var controllerName = actionExecutedContext.ActionContext?.ControllerContext?.Controller?.GetType()?.FullName;

            var keyValuePair = actionExecutedContext?.ActionContext?.ActionArguments?.FirstOrDefault();
            var request = keyValuePair?.Value as IRequest;

            var logger = _logger.ForContext("StatusCode", statusCode)
                                .ForContextIfNotNull("MediaType", mediaType)
                                .ForContextIfNotNull("ResponseContent", request?.GetResponseContentForLogging((string)Serialize(dynamicResponseContent.Value), mediaType))
                                .ForContextIfNotNull("Controller", controllerName);

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
