// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerilogExtensions.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi
{
    using Serilog;

    internal static class SerilogExtensions
    {
        public static ILogger ForContextIfNotNull(this ILogger logger, string propertyName, object value, bool destructureObjects = false)
        {
            if (value == null)
                return logger;

            return logger.ForContext(propertyName, value, destructureObjects);
        }
    }
}