using Orleans.Concurrency;
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
                .ShouldContain(x => x.Name == "ExplicitTestMethod", 1);
        }

        [Fact]
        public void Should_return_inherited_interface_method()
        {
            ReflectionUtil.GetMethodsIncludingBaseInterfaces(typeof(ITestGrain3))
                .ShouldContain(x => x.Name == "IntNoParameters", 1);
        }

        [Fact]
        public void Can_create_immutableT()
        {
            var creator = ReflectionUtil.GetObjectActivator(typeof(Immutable<>).MakeGenericType(typeof(int)),
                typeof(int));

            var instance = creator(5);

            ((Immutable<int>)instance).Value.ShouldBe(5);

        }

        [Fact]
        public void Can_get_value_dynamic()
        {
            var getter = ReflectionUtil.GetValueGetter(typeof(Immutable<>).MakeGenericType(typeof(int)), "Value");

            var value = getter(new Immutable<int>(5));

            value.ShouldBe(5);
        }
    }
}