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
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Gone;
    }
}