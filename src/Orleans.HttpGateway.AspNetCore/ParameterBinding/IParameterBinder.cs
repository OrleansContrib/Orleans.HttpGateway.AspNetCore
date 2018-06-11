using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Orleans.HttpGateway.AspNetCore.ParameterBinding
{
    public interface IParameterBinder
    {
        Task<bool> CanBind(ParameterInfo[] parameters, HttpRequest context);

        Task<object[]> BindParameters(ParameterInfo[] parameters, HttpRequest context);
    }
}