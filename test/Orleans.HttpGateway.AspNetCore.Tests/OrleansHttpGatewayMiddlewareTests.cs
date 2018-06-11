using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Concurrency;
using Orleans.HttpGateway.AspNetCore.ParameterBinding;
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

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
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
            
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
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

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());

            ((bool)json.success).ShouldBeTrue();

            testGrain.Verify();
        }

        [Fact]
        public async Task Invoke_PostObjectWithComplexParameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.PostObjectWithComplexParameters(
                                            It.Is<ComplexParameter1>(p => p.Key == "sleutel" && p.Value == "waarde"),
                                            "somestring"))
                .Returns(() => Task.FromResult<object>(new
                {
                    success = true
                })).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            string jsonRequest = @"{  
   ""p1"":{  
      ""key"":""sleutel"",
      ""value"":""waarde""
   },
   ""p2"":""somestring""
}";
            var response = await _client.PostAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/PostObjectWithComplexParameters",
                new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                );


            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            testGrain.Verify();

            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(responseJson);

            ((bool)json.success).ShouldBeTrue();

        }

        [Fact]
        public async Task Invoke_PostObjectWithComplexImmutableParameters_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.PostObjectWithComplexImmutableParameters(
                    It.Is<Immutable<ComplexParameter1>>(p => p.Value.Key == "sleutel" && p.Value.Value == "waarde")))
                .Returns(() => Task.FromResult<object>(new
                {
                    success = new Immutable<int>(5)
                })).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            string jsonRequest = @"{  
   ""p1"":{  
      ""key"":""sleutel"",
      ""value"":""waarde""
   }}";
            var response = await _client.PostAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/PostObjectWithComplexImmutableParameters",
                new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            );

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

            testGrain.Verify();

            var responseJson = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(responseJson);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ImmutableConverter());
            var serializer = JsonSerializer.Create(settings);
            

            var result = json["success"].ToObject<Immutable<int>>(serializer);
            result.Value.ShouldBe(5);
        }

        [Fact]
        public async Task Invoke_PostNoParametersNoResponse_GET_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.PostNoParametersNoResponse()).Returns(() => Task.CompletedTask).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.GetAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/PostNoParametersNoResponse");

            response.EnsureSuccessStatusCode();



            testGrain.Verify();
        }

        [Fact]
        public async Task Invoke_PostNoParametersNoResponse_POST_Success()
        {
            var testGrain = new Mock<ITestGrain3>();
            testGrain.Setup(x => x.PostNoParametersNoResponse()).Returns(() => Task.CompletedTask).Verifiable();

            _factoryMock.Setup(x => x.GetGrain<ITestGrain3>(6, null)).Returns(testGrain.Object);

            var response = await _client.PostAsync(
                "Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/PostNoParametersNoResponse", null);

            response.EnsureSuccessStatusCode();



            testGrain.Verify();
        }

        public void Dispose()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }

}