using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using hub.Services;
using hub.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using shared.Extensions;


namespace hub.Controllers
{
    [Route("status")]
    public class StatusController : ControllerBase
    {
        private ILogger _logger { get; set;}
        private IHubContext<AgentHub, IAgentClient> _hub { get; set;}
        public StatusController(ILogger logger, IHubContext<AgentHub, IAgentClient> hub)
        {
            _hub = hub;
            _logger = logger;
        }
        
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Get()
        {
            string userId = this.HttpContext.GetPresalyticsUserId();
            try
            {
                await _hub.Clients.User(userId).AgentSync();
                return Ok();
            } 
            catch (Exception ex)
            {   
                string msg = string.Format("Error Requesting Agent Status for user {user_id}", userId);
                Log.Error(ex, msg);
                return BadRequest(msg);
            }
        }
    }
}