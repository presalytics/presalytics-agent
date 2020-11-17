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


namespace test_integration
{
    public class ConnectionTests : IClassFixture<HubFactory<hub.Startup>>, IClassFixture<ClientFactory<client.Startup>>
    {
        private readonly HubFactory<hub.Startup> _hubFactory;
        private readonly ClientFactory<client.Startup>  _clientFactory;

        private  dynamic _token;

        public ConnectionTests(HubFactory<hub.Startup> hubFactory, ClientFactory<client.Startup> clientFactory)
        {
            _hubFactory = hubFactory;
            _clientFactory = clientFactory;
            _token = TestToken.Get();
        }
        
        
        [Fact]
        public async Task SyncAgent_ReturnsAgentId()
        {
            var hubClient = _hubFactory.CreateClient();

            hubClient.SetFakeBearerToken((object)_token);

            HttpResponseMessage resp = await hubClient.GetAsync("/status");

            resp.EnsureSuccessStatusCode();

        }
    }
}
