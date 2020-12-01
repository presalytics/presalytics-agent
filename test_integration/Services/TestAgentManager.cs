using hub.Services;
using Serilog;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace test_integration.Services
{
    public class TestAgentManager : AgentManager
    {
        public IEventReportSink EventReportSink { get; set;}
        public TestAgentManager(ILogger logger, IHttpClientFactory clientFactory, IEventReportSink eventReportSink) : base(logger, clientFactory) 
        {
            EventReportSink = eventReportSink;
        }

        public override async Task SyncAgent(Guid agentId, string agentName)
        {
            await base.SyncAgent(agentId, agentName);
            EventReportSink.Report.SetResult(agentId.ToString());

        }
    }
}