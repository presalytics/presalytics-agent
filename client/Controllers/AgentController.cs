using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Serilog;
using client.Services;
using shared.Models;

namespace client.Controllers
{
    [Route("agent")]
    public class AgentController : ControllerBase
    {
        private ISocketBroker SocketBroker { get; set;}
        private ILogger Logger { get; set;}

        public AgentController(ISocketBroker socketBroker, ILogger logger)
        {
            SocketBroker = socketBroker;
            Logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Get()
        {
            try
            {
                WorkspaceAgent agent = await SocketBroker.Workspace.GetAgentAsync();
                return Ok(agent);
            } 
            catch (Exception ex)
            {   
                string msg = "Error When Attempting to Acquire AgentId from Workspace.  Is the workspace connected and running?";
                Log.Error(ex, msg);
                return BadRequest(msg);
            }
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Post([FromBody]WorkspaceAgent agent)
        {
            try
            {
                await SocketBroker.Socket.SyncAgent(agent);
                return Ok();
            }
            catch (Exception ex)
            {
                string msg = "Error While Syncing AgentId.  Is this client hub connected to the hub?";
                Log.Error(ex, msg);
                return BadRequest(msg);
            }
        }
    }
}