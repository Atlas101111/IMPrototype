using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ImPrototype.Hubs
{
    public class ConnectionManager: IUserIdProvider
    {
        private static readonly List<UserInfo> connections = new List<UserInfo>();

        public UserInfo GetConnection(string connectionId)
        {
            return connections.Find(x => x.ConnectionId.Equals(connectionId));
        }
        public UserInfo GetConnectionByName(string userName)
        {
            return connections.Find(x => x.UserName.Equals(userName));
        }
        public void RemoveConnectionByName(string userName)
        {
            var info = GetConnectionByName(userName);
            RemoveConnection(info);
        }

        public bool AddConnection(UserInfo userInfo)
        {
            try
            {
                connections.Add(userInfo);
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        public bool RemoveConnection(UserInfo userInfo)
        {
            try
            {
                connections.Remove(userInfo);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            throw new NotImplementedException();
        }
    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string ConnectionId { get; set; }
    }
}
