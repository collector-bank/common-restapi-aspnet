// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestResponseLoggerFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Collector.Common.Library.Correlation;

    using Microsoft.CSharp.RuntimeBinder;

    using Newtonsoft.Json;

    using Serilog;

    public class RequestResponseLoggerFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public RequestResponseLoggerFilter(ILogger logger)
        {
            _logger = logger.ForContext<RequestResponseLoggerFilter>();
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            dynamic request = actionContext.ActionArguments["request"];

            _logger.ForContext("RequestBody", Serialize(request)).Information("Request received");

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            dynamic response = actionExecutedContext.ActionContext.Response.Content;

            try
            {
                if (response.Value != null)
                {
                    dynamic request = actionExecutedContext.ActionContext.ActionArguments["request"];

                    CorrelationState.InitializeCorrelation(request.CorrelationId);

                    try
                    {
                        if (string.IsNullOrEmpty(response.Value.Id))
                        {
                            response.Value.Id = CorrelationState.GetCurrentCorrelationId().ToString();
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause - Work around if response does not have an Id property
                    catch
                    {
                    }

                    _logger.ForContext("ResponseBody", Serialize(response.Value)).Information("Response sent");
                }
            }
            catch (RuntimeBinderException exception)
            {
                _logger.Error(exception, "Could not log response");
            }

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }
    }
}
