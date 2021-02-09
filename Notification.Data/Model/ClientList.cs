using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Notification.Data.Model
{
    public interface IClientList
    {
        void CreateUser(string connectionId, string userId);
        void RemoveUser(string connectionId);
        void LatestPing(string connectionId);

        Client GetUser(string connectionId);

        IEnumerable<Client> GetClients();
    }

    public class ClientList : IClientList
    {
        private ConcurrentDictionary<string, Client> _users;
        public ClientList()
        {
            _users = new ConcurrentDictionary<string, Client>();
        }

        public void CreateUser(string connectionId, string userId)
        {
            if (!_users.TryAdd(connectionId, new Client(connectionId, userId)))
                throw new Exception("Couldn't add new user to the list.");
        }
        public void RemoveUser(string connectionId)
        {
            // I didn't remove the client from the collection for see the exit date
            if (_users.TryGetValue(connectionId, out var client))
                client.ExitTime = DateTime.Now;
        }
        public void LatestPing(string connectionId)
        {
            if (_users.TryGetValue(connectionId, out var client))
                client.LatestPingTime = DateTime.Now;
        }

        public IEnumerable<Client> GetClients() => _users.Values;

        public Client GetUser(string connectionId)
        {
            if (_users.TryGetValue(connectionId, out var client))
                return client;
            else
                return (null);
        }
    }
}
