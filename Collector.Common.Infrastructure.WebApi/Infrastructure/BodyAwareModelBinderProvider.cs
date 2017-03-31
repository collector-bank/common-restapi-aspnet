// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BodyAwareModelBinderProvider.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Infrastructure
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.ValueProviders.Providers;

    using Collector.Common.RestContracts.Interfaces;

    public class BodyAwareModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            return typeof(IRequest).IsAssignableFrom(modelType) ? new BodyAwareModelBinder() : null;
        }

        private class BodyAwareModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (bindingContext.Model != null)
                    return false;

                var request = actionContext.ActionArguments.SingleOrDefault().Value;

                bindingContext.Model = new RequestBuilder(bindingContext.ModelType, request)
                    .WithResourceIdentifier(new RouteDataValueProviderFactory().GetValueProvider(actionContext))
                    .Create();

                return request != null;
            }
        }
    }
}