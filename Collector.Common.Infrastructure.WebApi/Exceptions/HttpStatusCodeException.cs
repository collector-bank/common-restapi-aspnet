// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpStatusCodeException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System;
    using System.Net;

    using Collector.Common.Library.Retry;

    public abstract class HttpStatusCodeException : Exception, IRetrySuppressingException
    {
        protected HttpStatusCodeException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public abstract HttpStatusCode HttpStatusCode { get; }

        public string ErrorCode { get; set; }
    }
}