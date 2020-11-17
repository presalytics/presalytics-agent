using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using Moq.Contrib.HttpClient;

namespace test_integration.Setup
{
    public interface IHttpMessageHandler
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
    public static class MockHttpClientFactory
    {
        public static Mock<IHttpClientFactory> GetMockFactory()
        {
            var mockFactory = new Mock<IHttpClientFactory>();
            
            // Create client with Mocked HTTP Message Handler
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var websiteUrl = System.Environment.GetEnvironmentVariable("WEBSITE_URL");


            mockHttpMessageHandler
                .Protected()
                .As<IHttpMessageHandler>()
                .Setup<Task<HttpResponseMessage>>(x =>
                x.SendAsync(
                    It.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get &&
                        r.RequestUri == new Uri($"{websiteUrl}/user/agent/")
                    ), It.IsAny<CancellationToken>()
                )).ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{'name': 'test_agent,'agentId':'32fb15b6-c5d4-40f1-8ede-a64359a8fb1d'}]"),
                });
 
            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockFactory.Setup(_ => _.CreateClient("website")).Returns(client);

            return mockFactory;
        } 
    }
}