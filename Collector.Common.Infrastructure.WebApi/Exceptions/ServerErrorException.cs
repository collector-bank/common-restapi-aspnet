// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerErrorException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Exceptions
{
    using System.Net;

    public class ServerErrorException : HttpStatusCodeException
    {
        public ServerErrorException(string message)
            : base(message)
        {
        }

        public override HttpStatusCode HttpStatusCode => HttpStatusCode.InternalServerError;
    }
}