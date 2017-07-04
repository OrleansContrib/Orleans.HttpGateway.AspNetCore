using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Orleans.HttpGateway.AspNetCore.ParameterBinding
{
    class NamedQueryStringParameterBinder : IParameterBinder
    {
        readonly JsonSerializer _serializer;

        public NamedQueryStringParameterBinder(JsonSerializerSettings settings)
        {
            this._serializer = JsonSerializer.Create(settings);
        }


        public bool CanBind(ParameterInfo[] parameters, HttpContext context)
        {
            if (parameters.Length == context.Request.Query.Count)
            {
                //check parameter names
                var source = parameters.Select(x => x.Name).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
                return source.SetEquals(context.Request.Query.Select(x => x.Key));
            }

            return false;
        }

        public object[] BindParameters(ParameterInfo[] parameters, HttpContext context)
        {
            var result = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                //support named parameters in querystring
                if (context.Request.Query.TryGetValue(parameter.Name, out StringValues value))
                {
                    if (parameter.ParameterType.IsArray)
                    {
                        var elementType = Internal.ReflectionUtil.GetAnyElementType(parameter.ParameterType);
                        Array array = Array.CreateInstance(elementType, value.Count);
                        for (int p = 0; p < value.Count; p++)
                        {
                            array.SetValue( Convert(value[p], elementType),p);
                        }
                        result[i] = array;
                    }
                    else
                    {
                        result[i] = Convert(value[0], parameter.ParameterType);
                    }
                }
            }
            return result;
        }

        object Convert(string source, Type t)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(t);
            if (converter.CanConvertTo(t) && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(source);
            }
            else if (t == typeof(Guid))
            {
                return Guid.Parse(source);
            }

            //fallback to json serializer..
            using (var jsonTextReader = new JsonTextReader(new StringReader(source)))
            {
                return _serializer.Deserialize(jsonTextReader, t);
            }
        }
    }


}