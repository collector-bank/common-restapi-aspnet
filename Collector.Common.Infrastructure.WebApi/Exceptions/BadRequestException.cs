// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BadRequestException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System.Net;

    public class BadRequestException : HttpStatusCodeException
    {
        public BadRequestException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;
    }
}