using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            var mockWebsiteHttpMessageHandler = new Mock<HttpMessageHandler>();

            var websiteUrl = System.Environment.GetEnvironmentVariable("WEBSITE_URL");
            var workspaceUrl = System.Environment.GetEnvironmentVariable("WORKSPACE_URL");

            var userAgentTestResponse = new List<UserAgent>();
            userAgentTestResponse.Add(new UserAgent(){
                AgentId = Guid.NewGuid(),
                Priority = 1,
                Name = "test_agent"
            });

            mockWebsiteHttpMessageHandler
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
                    Content = new StringContent(JsonSerializer.Serialize(userAgentTestResponse))
                });
 
            var websiteClient = new HttpClient(mockWebsiteHttpMessageHandler.Object) {
                BaseAddress = new Uri(websiteUrl)
            };

            mockFactory.Setup(_ => _.CreateClient("website")).Returns(websiteClient);

            var mockWorkspaceHttpMessageHandler = new Mock<HttpMessageHandler>();

            var workspaceAgentTestResponse = new WorkspaceAgent() {
                Name = userAgentTestResponse[0].Name,
                AgentId = userAgentTestResponse[0].AgentId
            };

            mockWorkspaceHttpMessageHandler
                .Protected()
                .As<IHttpMessageHandler>()
                .Setup<Task<HttpResponseMessage>>(x => x.SendAsync(
                    It.Is<HttpRequestMessage>( req => 
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{workspaceUrl}/agent")
                ), It.IsAny<CancellationToken>()
                )).ReturnsAsync( new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(workspaceAgentTestResponse))
                });

            var workspaceClient = new HttpClient(mockWorkspaceHttpMessageHandler.Object) {
                BaseAddress = new Uri(workspaceUrl)
            };
            mockFactory.Setup(_ => _.CreateClient("workspace")).Returns(workspaceClient);

            return mockFactory;
        } 
    }
}