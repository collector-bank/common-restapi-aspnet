﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CorrelationIdActionFilter.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Filters
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using Library.Correlation;
    using Microsoft.CSharp.RuntimeBinder;


    public class CorrelationIdActionFilter : ActionFilterAttribute
    {
        private readonly bool _setCorrelationIdFromContext;

        public CorrelationIdActionFilter(bool setCorrelationIdFromContext = false)
        {
            _setCorrelationIdFromContext = setCorrelationIdFromContext;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Guid? existingCorrelationId = null;
            dynamic request = actionContext.ActionArguments.Values.FirstOrDefault();

            if (request != null)
            {
                if (_setCorrelationIdFromContext)
                {
                    try
                    {
                        existingCorrelationId = Guid.Parse(request.Context);
                    }
                    catch
                    {
                    }
                }
            }

            CorrelationState.InitializeCorrelation(existingCorrelationId);

            if (request != null)
                CorrelationState.TryAddOrUpdateCorrelationValue("CallerContext", request.Context);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            dynamic response = actionExecutedContext.ActionContext.Response?.Content;

            // Response can be null if we respond with streamed data (i.e. csv/pdf export)
            if (response != null)
            {
                // This method is always executed after each request but we can't guarantee
                // the thread context. If correlation Id is set in another thread in case
                // of exception - correlation id will not be set for this thread and we
                // shouldn't replace it.
                try
                {
                    if (string.IsNullOrEmpty(response.Value.CorrelationId))
                    {
                        response.Value.CorrelationId = CorrelationState.GetCurrentCorrelationId()?.ToString();
                    }

                    actionExecutedContext.ActionContext.Response.Content = response;
                }
                catch (RuntimeBinderException)
                {
                }
            }

            CorrelationState.ClearCorrelation();

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}