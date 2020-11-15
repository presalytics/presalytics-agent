using System;
using Xunit;
using System.Threading.Tasks;
using test_integration.Setup;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using hub;
using client;


namespace test_integration
{
    public class ConnectionTests : IClassFixture<HubFactory<hub.Startup>>, IClassFixture<ClientFactory<client.Startup>>
    {
        private readonly HubFactory<hub.Startup> _hubFactory ;
        private readonly ClientFactory<client.Startup>  _clientFactory ;
        public ConnectionTests(HubFactory<hub.Startup> hubFactory, ClientFactory<client.Startup> clientFactory)
        {
            _hubFactory = hubFactory;
            _clientFactory = clientFactory;
        }
        
        
        [Fact]
        public async Task SyncAgent_ReturnsAgentId()
        {
            var hubClient = _hubFactory.CreateClient();

            HttpResponseMessage resp = await hubClient.GetAsync("/status");

            resp.EnsureSuccessStatusCode();

        }
    }
}
