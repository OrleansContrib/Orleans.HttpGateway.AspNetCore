using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Orleans.HttpGateway.AspNetCore.Internal;

namespace Orleans.HttpGateway.AspNetCore
{
    internal class ExpressionBasedGrainReferenceProvider : IGrainReferenceProvider
    {

        private readonly IGrainFactory _grainFactory;
        private readonly ConcurrentDictionary<Type, Func<string, object>> _cachedFactoryMethods = new ConcurrentDictionary<Type, Func<string, object>>();

        public ExpressionBasedGrainReferenceProvider(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        private readonly Tuple<Type, MethodInfo>[] _grainIdentityInterfaceMap =
            typeof(IGrainFactory)
                .GetMethods()
                .Where(x => x.Name == "GetGrain" && x.IsGenericMethod)
                .Select(x => Tuple.Create(x.GetGenericArguments()[0].GetGenericParameterConstraints()[0], x)).ToArray();


        private Func<string, object> BuildFactoryMethod(Type grainType)
        {
            var mi = _grainIdentityInterfaceMap.FirstOrDefault(x => x.Item1.IsAssignableFrom(grainType));

            if (mi != null)
            {
                var factoryDelegate =
                    DelegateFactory.Create(mi.Item2.GetGenericMethodDefinition().MakeGenericMethod(grainType));
                var idParser = GetArgumentParser(mi.Item2.GetParameters());
                return (id) => factoryDelegate(_grainFactory, idParser(id));
            }
            throw new NotSupportedException($"cannot construct grain {grainType.Name}");
        }

        private Func<string, object[]> GetArgumentParser(ParameterInfo[] parameters)
        {
            string[] idseperator = new[] { "," };

            return (id) =>
            {
                var idParts = id.Split(idseperator, StringSplitOptions.RemoveEmptyEntries);
                object[] values = new object[parameters.Length];
                for (int i = 0; i < idParts.Length; i++)
                {
                    values[i] = TryParse(idParts[i], parameters[i].ParameterType);
                }

                return values;
            };
        }

        static object TryParse(string source, Type t)
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
            throw new ArgumentException($"Can't parse '{source}' as {t.FullName}", nameof(source));
        }

        public object GetGrainReference(Type grainType, string id)
        {
            return this.GetGrainFactoryMethod(grainType)(id);
        }

        private Func<string, object> GetGrainFactoryMethod(Type grainType)
        {
            return _cachedFactoryMethods.GetOrAdd(grainType, BuildFactoryMethod);
        }
    }
}