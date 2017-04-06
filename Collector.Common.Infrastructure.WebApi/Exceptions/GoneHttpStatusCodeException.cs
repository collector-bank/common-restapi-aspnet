// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoneHttpStatusCodeException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System.Net;

    public class GoneHttpStatusCodeException : HttpStatusCodeException
    {
        public GoneHttpStatusCodeException(string errorCode)
            : base(errorCode)
        {
        }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Gone;
    }
}