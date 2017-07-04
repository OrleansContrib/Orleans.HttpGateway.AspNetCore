using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleans.HttpGateway.AspNetCore.Internal
{
    internal static class ReflectionUtil

    {
        public static IEnumerable<MethodInfo> GetMethodsIncludingBaseInterfaces(Type t)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;


            foreach (var mi in t.GetMethods())
            {
                yield return mi;
            }
            
            foreach (Type interf in t.GetInterfaces())
            {
                foreach (MethodInfo method in interf.GetMethods(flags))
                    yield return method;
            }
        }

        public static Type GetAnyElementType(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                .Where(t => t.IsGenericType &&
                            t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return enumType ?? type;
        }
    }
}