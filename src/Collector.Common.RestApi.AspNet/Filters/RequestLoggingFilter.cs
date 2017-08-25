namespace Collector.Common.RestApi.AspNet.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.RestContracts;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Serilog;

    using HttpMethod = System.Net.Http.HttpMethod;

    public class RequestLoggingFilter : ActionFilterAttribute
    {
        internal const string REQUEST_RECIEVED_TIME = "RequestRecievedTime";
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>> CachedSensitiveStrings = new ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>>();
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

                var sensitiveStrings = GetSensitiveStrings(actionContext);
                var rawRequestBody = ReadRequestContent(actionContext);
                var formattedRequestBody = FormatRequestBody(actionContext, rawRequestBody, sensitiveStrings);

                _logger.ForContextIfNotNull("RawRequestBody", sensitiveStrings.Any() ? "Request contains sensitive information" : rawRequestBody)
                       .ForContextIfNotNull("RequestBody", formattedRequestBody)
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

        private string FormatRequestBody(HttpActionContext actionContext, string value, IEnumerable<PropertyInfo> sensitiveStrings)
        {
            try
            {
                var jObject = (JObject)JsonConvert.DeserializeObject(value);
                foreach (var sensitiveString in sensitiveStrings)
                {
                    if(actionContext.Request.Method != HttpMethodPatch || jObject.GetValue(sensitiveString.Name) != null)
                        jObject[sensitiveString.Name] = new string('*', 10);
                }

                return JsonConvert.SerializeObject(jObject, Formatting.Indented);
            }
            catch
            {
                return "Could not format the raw request body";
            }
        }

        private IReadOnlyCollection<PropertyInfo> GetSensitiveStrings(HttpActionContext actionContext)
        {
            var requestPair = actionContext?.ActionArguments?.FirstOrDefault();
            // ReSharper disable once ConstantConditionalAccessQualifier, Resharper does not understand that we need the ?. operator here.
            var type = requestPair?.Value?.GetType();

            if (type == null)
                return new ReadOnlyCollection<PropertyInfo>(new List<PropertyInfo>());

            return CachedSensitiveStrings.GetOrAdd(
                key: type,
                valueFactory: newKey =>
                                  {
                                      return new ReadOnlyCollection<PropertyInfo>(type.GetProperties()
                                                 .Where(p => p.GetCustomAttributes(typeof(SensitiveStringAttribute), true).Any())
                                                 .ToList());
                                  });
        }
    }
}
