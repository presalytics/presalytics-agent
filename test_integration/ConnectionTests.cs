using System;
using Xunit;
using System.Threading.Tasks;
using test_integration.Setup;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using hub;
using client;
using WebMotions.Fake.Authentication.JwtBearer;
using System.Dynamic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using shared.Models;
using Microsoft.AspNetCore.Http;


namespace test_integration
{
    public class ConnectionTests : IClassFixture<ClientFactory<client.Startup>>
    {
        private readonly ClientFactory<client.Startup>  _clientFactory;

        private  dynamic _token;

        public ConnectionTests(ClientFactory<client.Startup> clientFactory)
        {
            _clientFactory = clientFactory;
            _token = TestToken.Get();
        }
        
        
        [Fact]
        public async Task SyncAgent_ReturnsAgentId()
        {

            //Setup
            // var clientClient = _clientFactory.CreateClient();

            // clientClient.SetFakeBearerToken((object)_token);
            
            var hubContainer = new TestHubContainer();

            hubContainer.Start();
            
            var hubclient = new HttpClient() {
                BaseAddress = new Uri(hubContainer.GetHostUrl())
            };

            hubclient.SetFakeBearerToken((object)_token);

            //Act
            //var agent = await clientClient.GetFromJsonAsync<WorkspaceAgent>("/agent");
            
            hubclient.GetAsync("/status").GetAwaiter().GetResult();

            var agentFromHub = await hubContainer.GetReport();

            //assert

            //Assert.Equal(agent.AgentId.ToString(), agentFromHub);
            


        }
    }
}
