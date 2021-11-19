using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ImPrototype.Hubs
{
    public class ConnectionManager: IUserIdProvider
    {

        private static readonly ConcurrentDictionary<string,string> connectionDict = new ConcurrentDictionary<string, string>();

        public string GetConnectionByName(string userName)
        {
            connectionDict.TryGetValue(userName, out var result);
            return result;
        }
        public void RemoveConnectionByName(string userName)
        {
            connectionDict.TryRemove(userName, out var outVal); 
        }
        public void RemoveConnectionById(string connectionId)
        {
            var itemToRemove = connectionDict.Where(kv => kv.Value.Equals(connectionId));
            foreach (var item in itemToRemove)
            {
                connectionDict.TryRemove(item.Key, out connectionId);
            }
        }
        public bool AddConnection(string accountUuid, string connectionId)
        {
            try
            {
                return connectionDict.TryAdd(accountUuid, connectionId);
            }
            catch(Exception e)
            {
                return false;
            }
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
