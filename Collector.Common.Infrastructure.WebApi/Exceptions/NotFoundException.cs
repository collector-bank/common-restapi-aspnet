// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotFoundException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System.Net;

    public class NotFoundException : HttpStatusCodeException
    {
        public NotFoundException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.NotFound;
    }
}