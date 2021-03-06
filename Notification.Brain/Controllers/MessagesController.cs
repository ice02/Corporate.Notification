using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Data.Context;
using Notification.Data.Dto;
using Notification.Data.Model;
using Swashbuckle.AspNetCore.Annotations;

namespace Notification.Brain.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly NotificationContext context;
        private HubConnection _hubConnection;

        public MessagesController(NotificationContext context)
        {
            this.context = context;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/notification", option => { option.UseDefaultCredentials = true; })
                .WithAutomaticReconnect()
                .ConfigureLogging(factory =>
                {
                    factory.AddConsole();
                    factory.AddFilter("Console", level => level >= LogLevel.Trace);
                }).Build();

            _hubConnection.StartAsync();
        }

        // GET api/messages
        [HttpGet("All")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Message>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await context.Messages.AsNoTracking().ToListAsync());
        }

        // GET api/messages?page={0}&pageSize={1}
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<Message>))]
        public async Task<IActionResult> GetPaged([FromQuery] int page, int pageSize)
        {
            return Ok(await context.Messages.AsNoTracking().GetPagedAsync(page, pageSize));
        }

        [HttpPost]
        //[Authorize()]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post(string toUser, string fromUser, [FromBody] MessageDto message)
        {
            if (message != null && !string.IsNullOrEmpty(toUser) && !string.IsNullOrEmpty(fromUser))
            {
                // TODO : translate DTO Message into SigRMessage
                
                await _hubConnection.SendAsync("SendToUser", message, toUser);
                //await _hubConnection.SendAsync("Send", new Message { Name = name, Text = message });
                //logger.LogInformation("SendAsync to Hub");
            }
            else
            {
                return base.ValidationProblem("a parameter is missing");
            }

            return Ok();
        }
    }
}