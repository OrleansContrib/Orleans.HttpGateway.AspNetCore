using System;
using Orleans.HttpGateway.AspNetCore;
using Orleans.HttpGateway.AspNetCore.GrainTypeProviders;
using Shouldly;
using Xunit;

namespace Orleans.HttpGateway.AspNetCore.Tests
{
    public class AssemblyBasedGrainTypeProviderTests
    {
        [Theory
        ,InlineData("Orleans.HttpGateway.AspNetCore.Tests.ITestGrain1", typeof(ITestGrain1))
        ,InlineData("Orleans.HttpGateway.AspNetCore.Tests.ITestGrain2", typeof(ITestGrain2))
        ,InlineData("Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3", typeof(ITestGrain3))
        ,InlineData("Orleans.HttpGateway.AspNetCore.Tests.ITestGrainNONEXISTENT", null)]
        public void Can_provide_TestGrainInterfaces(string name, Type excpected)
        {
            var sut = new AssemblyBasedGrainTypeProvider(typeof(AssemblyBasedGrainTypeProviderTests).Assembly);

            sut.GetGrainType(name).ShouldBe(excpected);
        }
    }
}