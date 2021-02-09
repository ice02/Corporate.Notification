using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Data.Context;
using Notification.Data.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace Notification.Engine.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> logger;
        private readonly NotificationContext notificationContext;
        private readonly IClientList _list;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notificationContext"></param>
        /// <param name="logger"></param>
        /// <param name="list"></param>
        public NotificationHub(NotificationContext notificationContext, ILogger<NotificationHub> logger, IClientList list)
        {
            this.notificationContext = notificationContext;
            this.logger = logger;
            _list = list;
        }

        
        //public Task Send(Message message)
        //{
        //    message.EventId = Event.Message.GetHashCode();
        //    message.CreatedDate = DateTime.Now;

        //    var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        //    if (userChat != null)
        //    {
        //        message.UserChatId = userChat.UserChatId;
        //        notificationContext.Messages.Add(message);
        //        notificationContext.SaveChangesAsync();
        //    }
        //    return Clients.All.SendAsync("ReceiveMessage", message);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        public Task SendtoUser(Message message, string user)
        {
            //message.EventId = Event.Message.GetHashCode();
            //message.CreatedDate = DateTime.Now;

            //var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //if (userChat != null)
            //{
            //    message.UserChatId = userChat.UserChatId;
            //    notificationContext.Messages.Add(message);
            //    notificationContext.SaveChangesAsync();
            //}

            var grp = Clients.Group($"usr-{user}");

            // TODO : if group doesn't exist, that mean user is not connected, put it in the cache

            return Clients.Group($"usr-{user}").SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            // register new user in his dedicated signalr group if not exist
            Groups.AddToGroupAsync(Context.ConnectionId, $"usr-{Context.User?.Identity?.Name}");
            _list.CreateUser(Context.ConnectionId, Context.User?.Identity?.Name);
            Heartbeat();

            logger.LogWarning($@"User ConnectionId: {Context.ConnectionId}  Name: {Context.User?.Identity?.Name} Identifier: {Context.UserIdentifier} connected.");

            return base.OnConnectedAsync();
        }

        //[Authorize]
        //public async Task OnUserConnected(Message message)
        //{
        //    try
        //    {
        //        message.EventId = Event.Connect.GetHashCode();
        //        message.CreatedDate = DateTime.Now;

        //        var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        //        if (userChat != null)
        //        {
        //            userChat.Name = message.Name;
        //            userChat.UpdatedDate = DateTime.Now;
        //            notificationContext.Update(userChat);
        //            await notificationContext.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            userChat = new UserChat
        //            {
        //                ConnectionId = Context.ConnectionId,
        //                Name = message.Name,
        //                CreatedDate = DateTime.Now
        //            };
        //            await notificationContext.AddAsync(userChat);
        //            await notificationContext.SaveChangesAsync();
        //        }

        //        message.UserChatId = userChat.UserChatId;
        //        message.Text = $"{message.Name} connected.";

        //        notificationContext.Messages.Add(message);
        //        await notificationContext.SaveChangesAsync();

        //        await Clients.All.SendAsync("UserConnected", message);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, ex.Message);
        //    }
        //}

        /// <summary>
        /// Register metadata for a user (country, language, ...)
        /// </summary>
        /// <param name="tags"></param>
        public void Register(Dictionary<string, string> tags )
        {
            //try
            //{
            //    Groups.AddToGroupAsync(Context.ConnectionId, $"usr-{clientName}");
            //    Clients.Group(clientName).SendAsync("RegistrationSuccess", $"{clientName} registered with Hub.");
            //    logger.LogWarning($@"User ConnectionId: {Context.ConnectionId}  Name: {Context.User?.Identity?.Name} Identifier: {Context.UserIdentifier} connected.");
            //}
            //catch (Exception ex)
            //{
            //    Clients.Group(clientName).SendAsync("Error", ex.Message);
            //    logger.LogError(ex, $"Register Failed at {DateTime.UtcNow} for client {clientName}");
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            logger.LogWarning($@"User ConnectionId: {Context.ConnectionId}  Name: {Context.User?.Identity?.Name} Identifier: {Context.UserIdentifier} disconnected.");

            var usr = _list.GetUser(Context.ConnectionId);
            if (usr != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"usr-{usr.UserId}");
                _list.RemoveUser(Context.ConnectionId);

                await notificationContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Heartbeat()
        {
            var heartbeat = Context.Features.Get<IConnectionHeartbeatFeature>();

            heartbeat.OnHeartbeat(state => {
                (HttpContext context, string connectionId) = ((HttpContext, string))state;
                //var ClientList = context.RequestServices.GetService<IClientList>();
                //ClientList.LatestPing(connectionId);
                _list.LatestPing(connectionId);
            }, (Context.GetHttpContext(), Context.ConnectionId));
        }

    }
}