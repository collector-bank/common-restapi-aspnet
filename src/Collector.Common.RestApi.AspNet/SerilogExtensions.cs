namespace Collector.Common.RestApi.AspNet
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