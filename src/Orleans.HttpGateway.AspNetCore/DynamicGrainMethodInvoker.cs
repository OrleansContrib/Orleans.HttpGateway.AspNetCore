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
           
            return await executor.ExecuteAsync(grain,
                GetParameters(executor, context));
        }

        private object[] GetParameters(ObjectMethodExecutor executor, HttpContext context)
        {
            //short circuit if no parameters
            if (executor.MethodParameters == null || executor.MethodParameters.Length == 0)
            {
                return Array.Empty<object>();
            }


            var suitableBinders = _parameterBinder.Where(x => x.CanBind(executor.MethodParameters, context));

            ExceptionDispatchInfo lastException = null;
            foreach (var binder in suitableBinders)
            {
                try
                {
                    return binder.BindParameters(executor.MethodParameters, context);
                }
                catch (Exception ex)
                {
                    //continue on next suitable binder
                    lastException = ExceptionDispatchInfo.Capture(ex);
                }
            }

            if (lastException != null)
            {
                lastException.Throw();
            }

            throw new InvalidOperationException("No suitable parameter binder found for request");
        }
    }


}