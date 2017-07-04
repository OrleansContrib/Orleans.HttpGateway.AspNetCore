using System;
using Moq;
using Orleans.HttpGateway.AspNetCore;
using Xunit;

namespace Orleans.HttpGateway.AspNetCore.Tests
{
    public class ExpressionBasedGrainReferenceProviderTests
    {

        [Fact]
        public void Should_invoke_correct_method_for_IGrainWithStringKey()
        {
            var grainFactory = new Mock<IGrainFactory>();

            var sut = new ExpressionBasedGrainReferenceProvider(grainFactory.Object);

            var result = sut.GetGrainReference(typeof(ITestGrain1), "key");

            grainFactory.Verify(x => x.GetGrain<ITestGrain1>("key", null), Times.Once);
        }

        [Fact]
        public void Should_invoke_correct_method_for_IGrainWithGuidKey()
        {
            var grainFactory = new Mock<IGrainFactory>();

            var sut = new ExpressionBasedGrainReferenceProvider(grainFactory.Object);

            var result = sut.GetGrainReference(typeof(ITestGrain2), "E3C2E8DA-88EF-4F78-B761-0AA442E6C9CC");

            grainFactory.Verify(x => x.GetGrain<ITestGrain2>(new Guid("E3C2E8DA-88EF-4F78-B761-0AA442E6C9CC"), null), Times.Once);
        }


        [Fact]
        public void Should_invoke_correct_method_for_IGrainWithIntegerKey()
        {
            var grainFactory = new Mock<IGrainFactory>();

            var sut = new ExpressionBasedGrainReferenceProvider(grainFactory.Object);

            var result = sut.GetGrainReference(typeof(ITestGrain3), "100");

            grainFactory.Verify(x => x.GetGrain<ITestGrain3>(100, null), Times.Once);
        }

        [Fact]
        public void Should_invoke_correct_method_for_IGrainWithIntegerCompoundKey()
        {
            var grainFactory = new Mock<IGrainFactory>();

            var sut = new ExpressionBasedGrainReferenceProvider(grainFactory.Object);

            var result = sut.GetGrainReference(typeof(ITestGrain4), "100,key");

            grainFactory.Verify(x => x.GetGrain<ITestGrain4>(100, "key",null), Times.Once);
           
        }

        [Fact]
        public void Should_invoke_correct_method_for_IGrainWithGuidCompoundKey()
        {
            var grainFactory = new Mock<IGrainFactory>();

            var sut = new ExpressionBasedGrainReferenceProvider(grainFactory.Object);

            var result = sut.GetGrainReference(typeof(ITestGrain5), "3FB93850-8806-4278-A476-CBE57A83FB50,key");

            grainFactory.Verify(x => x.GetGrain<ITestGrain5>(new Guid("3FB93850-8806-4278-A476-CBE57A83FB50"), "key", null), Times.Once);

        }
    }
}