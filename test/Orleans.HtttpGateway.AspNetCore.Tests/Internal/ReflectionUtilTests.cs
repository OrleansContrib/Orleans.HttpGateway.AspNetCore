using Orleans.HttpGateway.AspNetCore.Internal;
using Shouldly;
using Xunit;

namespace Orleans.HttpGateway.AspNetCore.Tests.Internal
{
    public class ReflectionUtilTests
    {
        [Fact]
        public void Should_return_interface_method()
        {
            ReflectionUtil.GetMethodsIncludingBaseInterfaces(typeof(ITestGrain3))
                .ShouldContain(x => x.Name == "ExplicitTestMethod",1);
        }

        [Fact]
        public void Should_return_inherited_interface_method()
        {
            ReflectionUtil.GetMethodsIncludingBaseInterfaces(typeof(ITestGrain3))
                .ShouldContain(x => x.Name == "IntNoParameters", 1);
        }
    }
}