// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestResponseLoggerFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.Library;
    using Collector.Common.Library.Collections;
    using Collector.Common.Library.Collections.Interfaces;
    using Collector.Common.RestContracts;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Serilog;

    public class RequestResponseLoggerFilter : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<Type, IFixedEnumerable<PropertyInfo>> CachedSensitiveStrings = new ConcurrentDictionary<Type, IFixedEnumerable<PropertyInfo>>();
        private const string REQUEST_RECIEVED_TIME = "RequestRecievedTime";
        private readonly ILogger _logger;

        public RequestResponseLoggerFilter(ILogger logger)
        {
            _logger = logger.ForContext<RequestResponseLoggerFilter>();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext?.Request?.Properties != null)
                    actionContext.Request.Properties[REQUEST_RECIEVED_TIME] = SystemTimeOffset.UtcNow;

                var sensitiveStrings = GetSensitiveStrings(actionContext);
                var rawRequestBody = ReadRequestContent(actionContext);
                var formattedRequestBody = FormatRequestBody(rawRequestBody, sensitiveStrings);

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

            var requestRecievedTime = actionExecutedContext.Request?.Properties?[REQUEST_RECIEVED_TIME] as DateTimeOffset?;
            if (requestRecievedTime.HasValue)
                logger = logger.ForContext("ResponseTimeMilliseconds", (int)(SystemTimeOffset.UtcNow - requestRecievedTime.Value).TotalMilliseconds);

            logger.Information("Rest response sent");
        }

        private static string ReadRequestContent(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method != System.Net.Http.HttpMethod.Post
                && actionContext.Request.Method != System.Net.Http.HttpMethod.Put)
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

        private static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        private string FormatRequestBody(string value, IEnumerable<PropertyInfo> sensitiveStrings)
        {
            try
            {
                var jObject = (JObject)JsonConvert.DeserializeObject(value);
                foreach (var sensitiveString in sensitiveStrings)
                    jObject[sensitiveString.Name] = new string('*', 10);

                return JsonConvert.SerializeObject(jObject, Formatting.Indented);
            }
            catch
            {
                return "Could not format the raw request body";
            }
        }

        private IFixedEnumerable<PropertyInfo> GetSensitiveStrings(HttpActionContext actionContext)
        {
            var requestPair = actionContext?.ActionArguments?.FirstOrDefault();
            // ReSharper disable once ConstantConditionalAccessQualifier, Resharper does not understand that we need the ?. operator here.
            var type = requestPair?.Value?.GetType();

            if (type == null)
                return FixedEnumerable<PropertyInfo>.Empty;

            return CachedSensitiveStrings.GetOrAdd(
                key: type,
                valueFactory: newKey =>
                                  {
                                      return type.GetProperties()
                                                 .Where(p => p.GetCustomAttributes(typeof(SensitiveStringAttribute), true).Any())
                                                 .ToFixed();
                                  });
        }
    }
}
