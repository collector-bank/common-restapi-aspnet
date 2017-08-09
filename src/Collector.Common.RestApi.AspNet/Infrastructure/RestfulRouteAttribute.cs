namespace Collector.Common.RestApi.AspNet.Infrastructure
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Web.Http.Routing;

    /// <summary>
    /// A routing attirbute that uses the controller methods name to match against a rest verb.
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