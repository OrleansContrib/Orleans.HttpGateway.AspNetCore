using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.HttpGateway.AspNetCore;
using Shouldly;
using Xunit;

namespace Orleans.HttpGateway.AspNetCore.Tests
{



    public class OrleansHttpGatewayMiddlewareTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly Mock<IGrainFactory> _factoryMock;

        public OrleansHttpGatewayMiddlewareTests()
        {
            _factoryMock = new Mock<IGrainFactory>();
            _server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOrleansHttpGateway(c => c.AddAssemblies(typeof(ITestGrain1).Assembly));
                    services.AddSingleton<IGrainFactory>(_factoryMock.Object);
                })
                .Configure(app => app.UseOrleansHttpGateway())
                );
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Invoke_IntNoParameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.IntNoParameters()).Returns(() => Task.FromResult(5)).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.GetAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/IntNoParameters");

            dynamic token = JToken.Parse(await response.Content.ReadAsStringAsync());

            int result = (int)token;
            result.ShouldBe(5);
            testGrain.Verify();
        }

        [Fact]
        public async Task Invoke_GetObjectWith2Parameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.GetObjectWith2Parameters(1, "okay")).Returns(() => Task.FromResult<object>(new
            {
                success = true
            })).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.GetAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/GetObjectWith2Parameters?one=1&two=okay");

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            ((bool)json.success).ShouldBeTrue();

            testGrain.Verify();
        }

        [Fact]
        public async Task Invoke_GetObjectWith2ArrayParameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.GetObjectWith2ArrayParameters(new[] { 1, 2 }, new[] { "okay", "dan" })).Returns(() => Task.FromResult<object>(new
            {
                success = true
            })).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.GetAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/GetObjectWith2ArrayParameters?one=1&one=2&two=okay&two=dan");

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            ((bool)json.success).ShouldBeTrue();

            testGrain.Verify();
        }


        [Fact]
        public async Task Invoke_GetObjectWithEnumerableParameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.GetObjectWithEnumerableParameters(new[] { 1, 2 })).Returns(() => Task.FromResult<object>(new
            {
                success = true
            })).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.GetAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/GetObjectWithEnumerableParameters?one=[1,2]");

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            ((bool)json.success).ShouldBeTrue();

            testGrain.Verify();
        }

        public void Dispose()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }

}