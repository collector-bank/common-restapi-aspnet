namespace Collector.Common.RestApi.AspNet.Infrastructure
{
    using System.Net;
    using System.Net.Http;

    using Collector.Common.RestContracts;

    public static class ResponseHelper
    {
        public static HttpResponseMessage BuildOkDataResponse<T>(this HttpRequestMessage request, T data)
        {
            var response = new Response<T>
            {
                Data = data
            };

            return request.CreateResponse(HttpStatusCode.OK, response);
        }

        public static HttpResponseMessage BuildOkVoidResponse(this HttpRequestMessage request)
        {
            var response = new Response<object>
            {
                Data = null
            };

            return request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}