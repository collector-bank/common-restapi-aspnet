// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextActionFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;

    using Microsoft.CSharp.RuntimeBinder;

    public class ContextActionFilter : ActionFilterAttribute
    {
        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            dynamic dynamicRequest = actionExecutedContext.ActionContext.ActionArguments.Values.FirstOrDefault();

            if (dynamicRequest != null)
            {
                try
                {
                    dynamic response = actionExecutedContext.ActionContext.Response?.Content;

                    // Response can be null if we respond with streamed data (i.e. csv/pdf export)
                    if (response != null)
                    {
                        response.Value.Context = dynamicRequest.Context;

                        actionExecutedContext.ActionContext.Response.Content = response;
                    }
                }
                catch (RuntimeBinderException)
                {
                }
            }

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}