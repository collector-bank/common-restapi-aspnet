namespace Collector.Common.RestApi.AspNet.Infrastructure
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

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

        public static HttpResponseMessage BuildOkStreamResponse(this HttpRequestMessage request, Stream stream, string mediaType)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            return response;
        }
    }
}