using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Notification.Data.Model;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Notification.Engine.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessageHubController<THub> : ControllerBase where THub:Hub
    {
        /// <summary>
        /// SignalR hub context.
        /// </summary>
        protected readonly IHubContext<THub> Hub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hub"></param>
        public MessageHubController(IHubContext<THub> hub)
        {
            Hub = hub;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost("")]
        [AllowAnonymous]
        //[Authorize()]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody] MessageToSend message)
        {
            var user = message.ToUser;

            var grp = Hub.Clients.Group($"usr-{user}");

            // TODO : if group doesn't exist, that mean user is not connected, put it in the cache

            await Hub.Clients.Group($"usr-{user}").SendAsync("ReceiveMessage", message);


            return Ok();
        }
    }
}
