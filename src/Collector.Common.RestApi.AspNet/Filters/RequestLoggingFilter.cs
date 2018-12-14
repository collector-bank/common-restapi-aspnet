namespace Collector.Common.RestApi.AspNet.Filters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.RestContracts.Interfaces;

    using Serilog;

    using HttpMethod = System.Net.Http.HttpMethod;

    public class RequestLoggingFilter : ActionFilterAttribute
    {
        internal const string REQUEST_RECIEVED_TIME = "RequestRecievedTime";
        private static readonly HttpMethod HttpMethodPatch = new HttpMethod("PATCH");
        private readonly ILogger _logger;
        
        public RequestLoggingFilter(ILogger logger)
        {
            _logger = logger.ForContext(GetType());
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext?.Request?.Properties != null)
                    actionContext.Request.Properties[REQUEST_RECIEVED_TIME] = DateTimeOffset.UtcNow;

                var rawRequestBody = ReadRequestContent(actionContext);
                var keyValuePair = actionContext?.ActionArguments?.FirstOrDefault();

                var request = keyValuePair?.Value as IRequest;

                _logger.ForContextIfNotNull("RawRequestContent", request?.GetRawRequestContentForLogging(rawRequestBody))
                       .ForContextIfNotNull("RequestContent", request?.GetRequestContentForLogging(rawRequestBody))
                       .ForContextIfNotNull("Controller", actionContext?.ControllerContext?.Controller?.GetType()?.FullName)
                       .Information("Rest request received");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not log that a rest request was received");
            }
        }

        private static string ReadRequestContent(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method != HttpMethod.Post
                && actionContext.Request.Method != HttpMethod.Put
                && actionContext.Request.Method != HttpMethodPatch)
                return string.Empty;

            using (var stream = new MemoryStream())
            {
                var context = (HttpContextBase)actionContext.Request.Properties["MS_HttpContext"];
                var position = context.Request.InputStream.Position;
                context.Request.InputStream.Seek(0, SeekOrigin.Begin);
                context.Request.InputStream.CopyTo(stream);
                context.Request.InputStream.Seek(position, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
