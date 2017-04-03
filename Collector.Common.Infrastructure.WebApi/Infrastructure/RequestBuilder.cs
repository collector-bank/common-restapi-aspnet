// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.Infrastructure.WebApi.Infrastructure
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Web.Http.ValueProviders;

    public class RequestBuilder
    {
        private readonly object _request;

        public RequestBuilder(Type requestType, object request = null)
        {
            var type = requestType;
            _request = request ?? (type != null ? FormatterServices.GetUninitializedObject(type) : null);
        }

        public RequestBuilder WithResourceIdentifier(IValueProvider valueProvider = null)
        {
            var fieldInfo = GetResourceIdentifierFromBaseClass(_request.GetType());
            if (fieldInfo == null)
                return this;

            if (fieldInfo.GetValue(_request) == null)
            {
                var resourceIdentifier = FormatterServices.GetUninitializedObject(fieldInfo.FieldType);
                fieldInfo.SetValue(_request, resourceIdentifier);
            }

            if (valueProvider != null)
            {
                var idProperties = fieldInfo.FieldType.GetProperties();
                foreach (var property in idProperties)
                {
                    var val = valueProvider.GetValue(property.Name);
                    if (val?.RawValue != null)
                    {
                        var parsedValue = TryParseValue(property, val);
                        if (parsedValue == null)
                            return this;

                        var resourceIdentifier = fieldInfo.GetValue(_request);
                        property.SetValue(resourceIdentifier, parsedValue, null);
                    }
                }
            }

            return this;
        }


        public object Create()
        {
            return _request;
        }

        private static object TryParseValue(PropertyInfo property, ValueProviderResult val)
        {
            try
            {
                var parsedValue = property.PropertyType == typeof(Guid)
                                           ? Guid.Parse(val.RawValue.ToString())
                                           : Convert.ChangeType(val.RawValue, property.PropertyType);
                return parsedValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static FieldInfo GetResourceIdentifierFromBaseClass(Type type)
        {
            while (type != null)
            {
                var field = type.GetField("_resourceIdentifier", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }
    }
}