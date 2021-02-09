using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Data.Context;
using Notification.Data.Model;
using Notification.Engine.Hubs;
using Swashbuckle.AspNetCore.Annotations;

namespace Notification.Engine.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly NotificationContext context;
        private HubConnection _hubConnection;
        //private IHubContext<NotificationHub> _hub;

        /// <summary>
        /// SignalR hub context.
        /// </summary>
        protected readonly IHubContext<NotificationHub> Hub;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public MessagesController(NotificationContext context, IHubContext<NotificationHub> hub)
        {
            this.context = context;
            Hub = hub;

            //_hubConnection = new HubConnectionBuilder()
            //    .WithUrl("https://localhost:5501/notification", option => { option.UseDefaultCredentials = true; })
            //    .WithAutomaticReconnect()
            //    .ConfigureLogging(factory =>
            //    {
            //        factory.AddConsole();
            //        factory.AddFilter("Console", level => level >= LogLevel.Trace);
            //    }).Build();

            //_hubConnection.StartAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/messages
        [HttpGet("All")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Message>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await context.Messages.AsNoTracking().ToListAsync());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET api/messages?page={0}&pageSize={1}
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<Message>))]
        public async Task<IActionResult> GetPaged([FromQuery] int page, int pageSize)
        {
            return Ok(await context.Messages.AsNoTracking().GetPagedAsync(page, pageSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toUser"></param>
        /// <param name="fromUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        //[HttpPost("from/{fromUser}/to/{toUser}")]
        ////[Authorize()]
        //[SwaggerResponse((int)HttpStatusCode.OK)]
        //public async Task<IActionResult> Post(string toUser, string fromUser, [FromBody] Message message)
        //{
        //    if (message != null && !string.IsNullOrEmpty(toUser) && !string.IsNullOrEmpty(fromUser))
        //    {
        //        await _hubConnection.SendAsync("SendToUser", message, toUser);
        //        //await _hub.Clients.Caller.SendAsync("ReceiveMessage", message);

        //        // TODO: log trace message
        //        //logger.LogTrace("SendAsync to Hub");
        //    }
        //    else
        //    {
        //        return base.ValidationProblem("a parameter is missing");
        //    }

        //    return Ok();
        //}

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