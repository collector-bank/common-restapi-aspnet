// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotImplementedHttpStatusCodeException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System.Net;

    public class NotImplementedHttpStatusCodeException : HttpStatusCodeException
    {
        public NotImplementedHttpStatusCodeException(string errorCode)
            : base(errorCode)
        {
        }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.NotImplemented;
    }
}