using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Orleans.HttpGateway.AspNetCore
{
    public interface IDynamicGrainMethodInvoker
    {
        Task<object> Invoke(Type grainType, object grain, GrainRouteValues grainRouteValues, HttpContext context);
    }


}