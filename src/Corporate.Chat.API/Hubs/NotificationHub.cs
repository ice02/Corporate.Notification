using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Corporate.Chat.API.Context;
using Corporate.Chat.API.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Corporate.Chat.API.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> logger;
        private readonly NotificationContext notificationContext;
        public NotificationHub(NotificationContext notificationContext, ILogger<NotificationHub> logger)
        {
            this.notificationContext = notificationContext;
            this.logger = logger;
        }

        public Task Send(Message message)
        {
            message.EventId = Event.Message.GetHashCode();
            message.CreatedDate = DateTime.Now;

            var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (userChat != null)
            {
                message.UserChatId = userChat.UserChatId;
                notificationContext.Messages.Add(message);
                notificationContext.SaveChangesAsync();
            }
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        public Task SendtoUser(Message message, string user)
        {
            message.EventId = Event.Message.GetHashCode();
            message.CreatedDate = DateTime.Now;

            var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (userChat != null)
            {
                message.UserChatId = userChat.UserChatId;
                notificationContext.Messages.Add(message);
                notificationContext.SaveChangesAsync();
            }
            return Clients.Group($"usr-{user}").SendAsync("ReceiveMessage", message);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task OnUserConnected(Message message)
        {
            try
            {
                message.EventId = Event.Connect.GetHashCode();
                message.CreatedDate = DateTime.Now;

                var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

                if (userChat != null)
                {
                    userChat.Name = message.Name;
                    userChat.UpdatedDate = DateTime.Now;
                    notificationContext.Update(userChat);
                    await notificationContext.SaveChangesAsync();
                }
                else
                {
                    userChat = new UserChat();
                    userChat.ConnectionId = Context.ConnectionId;
                    userChat.Name = message.Name;
                    userChat.CreatedDate = DateTime.Now;
                    await notificationContext.AddAsync(userChat);
                    await notificationContext.SaveChangesAsync();
                }

                message.UserChatId = userChat.UserChatId;
                message.Text = $"{message.Name} connected.";

                notificationContext.Messages.Add(message);
                await notificationContext.SaveChangesAsync();

                // register new user in his dedicated signalr group if not exist
                await Groups.AddToGroupAsync(Context.ConnectionId, $"usr-{message.Name}");

                logger.LogWarning($@"User ConnectionId: {Context.ConnectionId}  Name: {Context.User?.Identity?.Name} Identifier: {Context.UserIdentifier} connected.");

                await Clients.All.SendAsync("UserConnected", message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
        
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            logger.LogWarning($@"User ConnectionId: {Context.ConnectionId}  Name: {Context.User?.Identity?.Name} Identifier: {Context.UserIdentifier} disconnected.");

            var userChat = notificationContext.UsersChat.AsNoTracking().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (userChat != null)
            {
                var message = new Message()
                {
                    Text = $"{userChat.Name} left.",
                    Name = userChat.Name,
                    CreatedDate = DateTime.Now,
                    UserChatId = userChat.UserChatId,
                    EventId = Event.Disconnect.GetHashCode()
                };

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"usr-{message.Name}");

                notificationContext.Messages.Add(message);
                await notificationContext.SaveChangesAsync();

                await Clients.All.SendAsync("UserDisconnected", message);
            }
        }

    }
}