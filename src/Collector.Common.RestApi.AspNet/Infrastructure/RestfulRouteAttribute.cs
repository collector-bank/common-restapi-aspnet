namespace Collector.Common.RestApi.AspNet.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Web.Http.Routing;

    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using HttpMethod = System.Net.Http.HttpMethod;

    /// <summary>
    /// A routing attribute that uses the controller methods name to match against a rest verb.
    /// </summary>
    public class RestfulRequestBaseRouteAttribute : RouteFactoryAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="methodName"></param>
        public RestfulRequestBaseRouteAttribute(string template, [CallerMemberName] string methodName = null)
            : base(template)
        {
            Constraints = new HttpRouteValueDictionary { { "method", new MethodConstraint(new HttpMethod(methodName)) } };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        public RestfulRequestBaseRouteAttribute(Type requestType)
            // ReSharper disable once ExplicitCallerInfoArgument
            : this(GetTemplate(requestType), GetHttpMethod(requestType))
        {
        }

        private static string GetHttpMethod(Type requestType)
        {
            var theGetHttpMethodMethodInfo = GetTheGetHttpMethodMethodInfo(requestType);

            if (theGetHttpMethodMethodInfo == null)
            {
                throw new NullReferenceException($"invalid configuration on object type {requestType.FullName}");
            }

            return theGetHttpMethodMethodInfo.Invoke(FormatterServices.GetUninitializedObject(requestType), null).ToString();
        }

        /// <summary>
        /// Will build the template from the resource identifier, and replace properties with real names, if needed.
        /// If the resource identifier contains one property with name someResourceId, then
        /// the identifier uri : /api/someresource/0/somesubresource/ => /api/somresource/{someResourceId}/{somesubresource}
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        private static string GetTemplate(Type requestType)
        {
            var fieldInfo = GetResourceIdentifierFromBaseClass(requestType);
            var resourceIdentifier = FormatterServices.GetUninitializedObject(fieldInfo.FieldType) as IResourceIdentifier;

            if (resourceIdentifier == null)
            {
                throw new ArgumentNullException($"Could not initalize resource identifier for type {requestType.FullName}");
            }

            var uri = resourceIdentifier.Uri;

            foreach (var propertyInfo in resourceIdentifier.GetType().GetProperties().Where(info => info.Name != nameof(resourceIdentifier.Uri)))
            {
                var defaultData = FormatterServices.GetUninitializedObject(propertyInfo.PropertyType);
                uri = ReplaceFirstOccurrence(uri, defaultData.ToString(), $"{{{propertyInfo.Name}}}");
            }

            return uri;
        }

        private static FieldInfo GetResourceIdentifierFromBaseClass(Type type)
        {
            while (type != null)
            {
                var field = type.GetField("_resourceIdentifier", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        private static MethodInfo GetTheGetHttpMethodMethodInfo(Type type)
        {
            while (type != null)
            {
                var method = type.GetMethod("GetHttpMethod");
                if (method != null)
                    return method;

                type = type.BaseType;
            }

            return null;
        }

        private static string ReplaceFirstOccurrence(string original, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(original))
                return string.Empty;
            if (string.IsNullOrEmpty(oldValue))
                return original;
            if (string.IsNullOrEmpty(newValue))
                newValue = string.Empty;
            var loc = original.IndexOf(oldValue, StringComparison.Ordinal);
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        /// <summary>
        /// Gets the route constraints.
        /// </summary>
        public override IDictionary<string, object> Constraints { get; }

        private class MethodConstraint : IHttpRouteConstraint
        {
            private readonly HttpMethod _httpMethod;

            public MethodConstraint(HttpMethod method)
            {
                _httpMethod = method;
            }

            public bool Match(
                HttpRequestMessage request,
                IHttpRoute route,
                string parameterName,
                IDictionary<string, object> values,
                HttpRouteDirection routeDirection)
            {
                return request.Method == _httpMethod;
            }
        }
    }

    /// <summary>
    /// A routing attribute that uses the controller methods name to match against a rest verb.
    /// </summary>
    public class RestfulRouteAttribute : RouteFactoryAttribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="RestfulRouteAttribute"/>
        /// </summary>
        /// <param name="template">The uri template to match against.</param>
        /// <param name="methodName">DO NOT SET THIS EXPLICITLY, this parameter will be automatically set.</param>
        public RestfulRouteAttribute(string template, [CallerMemberName] string methodName = null)
            : base(template)
        {
            Constraints = new HttpRouteValueDictionary { { "method", new MethodConstraint(new HttpMethod(methodName)) } };
        }

        /// <summary>
        /// Gets the route constraints.
        /// </summary>
        public override IDictionary<string, object> Constraints { get; }

        private class MethodConstraint : IHttpRouteConstraint
        {
            private readonly HttpMethod _httpMethod;

            public MethodConstraint(HttpMethod method)
            {
                _httpMethod = method;
            }

            public bool Match(
                HttpRequestMessage request,
                IHttpRoute route,
                string parameterName,
                IDictionary<string, object> values,
                HttpRouteDirection routeDirection)
            {
                return request.Method == _httpMethod;
            }
        }
    }
}
