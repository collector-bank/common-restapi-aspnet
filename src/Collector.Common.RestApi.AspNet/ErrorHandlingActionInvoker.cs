namespace Collector.Common.Infrastructure.WebApi
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;

    using Collector.Common.RestContracts;

    using Serilog;

    public abstract class ErrorHandlingActionInvoker : ApiControllerActionInvoker
    {
        private readonly ILogger _logger;

        protected ErrorHandlingActionInvoker(ILogger logger)
        {
            _logger = logger.ForContext(GetType());
        }

        /// <summary>
        /// Asynchronously invokes the specified action by using the specified controller context.
        /// </summary>
        /// <param name="actionContext">The controller context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The invoked action.
        /// </returns>
        public override async Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var httpResponseMessage = await base.InvokeActionAsync(actionContext, cancellationToken);
                return httpResponseMessage.IsSuccessStatusCode
                           ? httpResponseMessage
                           : CreateCustomHttpStatusCodeResponse(actionContext, httpResponseMessage.StatusCode);
            }
            catch (Exception e)
            {
                return CreateErrorResponse(actionContext, e);
            }
        }

        public HttpResponseMessage CreateErrorResponse(HttpActionContext actionContext, Exception exception)
        {
            var errorCode = GetErrorCode(exception);
            if (!string.IsNullOrEmpty(errorCode))
                return CreateUnprocessableEntityResponse(actionContext, errorCode);

            LogException(actionContext, exception);

            return CreateCustomHttpStatusCodeResponse(actionContext, HttpStatusCode.InternalServerError);
        }

        protected abstract string GetErrorCode(Exception exception);

        private static HttpResponseMessage CreateCustomHttpStatusCodeResponse(HttpActionContext actionContext, HttpStatusCode httpStatusCode)
        {
            return actionContext.Request.CreateResponse(
                httpStatusCode,
                new Response<object>
                {
                    Error = new Error
                            {
                                Message = httpStatusCode.ToString(),
                                Code = $"{(int)httpStatusCode}"
                            }
                });
        }

        private static HttpResponseMessage CreateUnprocessableEntityResponse(HttpActionContext actionContext, string errorcode)
        {
            return actionContext.Request.CreateResponse(
                (HttpStatusCode)422,
                new Response<object>
                {
                    Error = new Error
                            {
                                Message = "Unprocessable Entity",
                                Code = "422",
                                Errors = new[]
                                         {
                                             new ErrorInfo
                                             {
                                                 Message = "BUSINESS_VIOLATION",
                                                 Reason = errorcode
                                             }
                                         }
                            }
                });
        }

        private void LogException(HttpActionContext actionContext, Exception baseException)
        {
            _logger.Error(baseException, "Critical exception occured while processing request in controller {Controller}", actionContext.ControllerContext.ControllerDescriptor.ControllerName);
        }
    }
}