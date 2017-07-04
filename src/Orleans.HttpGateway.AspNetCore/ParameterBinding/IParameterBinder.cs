using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Orleans.HttpGateway.AspNetCore.ParameterBinding
{
    public interface IParameterBinder
    {
        bool CanBind(ParameterInfo[] parameters, HttpContext context);

        object[] BindParameters(ParameterInfo[] parameters, HttpContext context);
    }


}