using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Linq;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Internal;
using Orleans.HttpGateway.AspNetCore.ParameterBinding;

namespace Orleans.HttpGateway.AspNetCore
{
    public class DynamicGrainMethodInvoker : IDynamicGrainMethodInvoker
    {
        readonly IParameterBinder[] _parameterBinder;
        readonly ConcurrentDictionary<string, ObjectMethodExecutor> _cachedExecutors = new ConcurrentDictionary<string, ObjectMethodExecutor>();

        public DynamicGrainMethodInvoker(IEnumerable<IParameterBinder> parameterBinders)
        {
            _parameterBinder = parameterBinders.ToArray();
        }

        public async Task<object> Invoke(Type grainType, object grain, GrainRouteValues grainRouteValues, HttpContext context)
        {
            var executor = _cachedExecutors.GetOrAdd($"{grainType.FullName}.{grainRouteValues.GrainMethod}",
                (key) =>
                {
                    var mi = Internal.ReflectionUtil.GetMethodsIncludingBaseInterfaces(grainType)
                        .FirstOrDefault(x => string.Equals(x.Name, grainRouteValues.GrainMethod));
                    return ObjectMethodExecutor.Create(mi, grainType.GetTypeInfo());
                });

            var parameters = await GetParameters(executor, context.Request);

            return await executor.ExecuteAsync(grain, parameters);
        }

        private async Task<object[]> GetParameters(ObjectMethodExecutor executor, HttpRequest request)
        {
            //short circuit if no parameters
            if (executor.MethodParameters == null || executor.MethodParameters.Length == 0)
            {
                return Array.Empty<object>();
            }

            // loop through binders, in order
            // first suitable binder wins
            // so the order of registration is important

            ExceptionDispatchInfo lastException = null;
            foreach (var binder in _parameterBinder)
            {
                try
                {
                    if (await binder.CanBind(executor.MethodParameters, request))
                    {
                        return await binder.BindParameters(executor.MethodParameters, request);
                    }
                }
                catch (Exception ex)
                {
                    // continue on next suitable binder
                    // but keep the exception when no other suitable binders are found
                    lastException = ExceptionDispatchInfo.Capture(ex);
                }
            }

            lastException?.Throw();

            throw new InvalidOperationException("No suitable parameter binder found for request");
        }
    }


}