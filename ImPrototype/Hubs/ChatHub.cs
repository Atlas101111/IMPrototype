using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using ImPrototype.Models;

namespace ImPrototype.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConnectionManager _connectionManager = UnityConfig.GetConfiguredContainer().Resolve<ConnectionManager>();
        private static readonly OfflineMongoAccessor _offlineMongoAccessor = UnityConfig.GetConfiguredContainer().Resolve<OfflineMongoAccessor>();

        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var id = Context.UserIdentifier;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _connectionManager.RemoveConnectionById(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageP2P(string user, string destUser, string message, string uniqueKey)
        {
            var connectionId = _connectionManager.GetConnectionByName(destUser);
            var newMessage = new ChatMessage
            {
                From = user,
                To = destUser,
                Content = message,
                Type = MessageType.Text,
                UniqueKey = uniqueKey
            };
            var lastMessage =  _offlineMongoAccessor.GetLastOfflineMessages(destUser);
            if (lastMessage != null && lastMessage.UniqueKey.Equals(uniqueKey))
            {
                // duplicate msg
                return;
            }

            var persisted = await _offlineMongoAccessor.SaveOfflineMessage(newMessage, destUser);
            if (!string.IsNullOrEmpty(connectionId) && persisted)
            {
                // destUser online

                await Clients.Client(connectionId).SendAsync("ReceiveMessage", newMessage);
            }
            else if (!persisted){
                throw new HubException("Persisted failed, please re-send");
            }
        }

        public void Login(string userName)
        {
            _connectionManager.AddConnection(userName, Context.ConnectionId);
        }

        public void Logout(string userName)
        {
            _connectionManager.RemoveConnectionByName(userName);
        }

        public async Task PullOffline(PullOfflineMessageRequest request)
        {
            try
            {
                var connectionId = _connectionManager.GetConnectionByName(request.AccountUuid);
                var chatMessages = new List<ChatMessage>();
                if (string.IsNullOrEmpty(connectionId))
                {
                    //have not login 
                    Login(request.AccountUuid);
                    return;
                }

                if(request.Offset == 0)
                {
                    // No local records, pull all offline messages
                    chatMessages =  _offlineMongoAccessor.GetAllOfflineMessages(request.AccountUuid);
                }
                else
                {
                    chatMessages =  _offlineMongoAccessor.GetOfflineMessages(request.AccountUuid, request.Offset, request.Size);
                }

                if(chatMessages.Count > 0)
                {
                    var maxMessageId = chatMessages.OrderBy(x => x.MessageId).ToList().Last().MessageId;
                    await Clients.Client(connectionId).SendAsync("ReceiveBatchMessages", chatMessages, maxMessageId, request.RequestId);
                }
            }
            catch(Exception e)
            {
                //return new PullOfflineMessageResponse { ChatMessages = new List<ChatMessage>() };
                return;
            }
        }

        public void ACK(ACKMessageRequest request)
        {
            try
            {
                _offlineMongoAccessor.ACKMessages(request.AccountUuid, request.ExpectMessageId);
            }
            catch(Exception e)
            {
                return;
            }
        }
    }

}