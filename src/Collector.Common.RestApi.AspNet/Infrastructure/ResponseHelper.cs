namespace Collector.Common.Infrastructure.WebApi.Infrastructure
{
    using System.Net;
    using System.Net.Http;

    using Collector.Common.RestContracts;

    public static class ResponseHelper
    {
        public static HttpResponseMessage BuildResponse<T>(this HttpRequestMessage request, T data, HttpStatusCode status = HttpStatusCode.OK, string apiVersion = null)
        {
            var response = new Response<T>
            {
                ApiVersion = apiVersion,
                Data = data
            };

            return request.CreateResponse(status, response);
        }
    }
}