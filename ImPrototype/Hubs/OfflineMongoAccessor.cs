using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using ImPrototype.Models;

namespace ImPrototype.Hubs
{
    public class OfflineMongoAccessor
    {
        private static readonly ImMongoClient imMongoClient = UnityConfig.GetConfiguredContainer().Resolve<ImMongoClient>();

        private const string DatabaseName = "im";
        private const string CollectionName = "OfflineMessages";

        private readonly IMongoCollection<OfflineMailbox> _offlineMessageCollection;
        public OfflineMongoAccessor(ImMongoClient mongoClient)
        {
            var client = mongoClient.GetClient();
            var database = client.GetDatabase(DatabaseName);

            var ignoreExtraElementsConvention = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
            };
            ConventionRegistry.Register("IgnoreExtraElements", ignoreExtraElementsConvention, type => true);

            _offlineMessageCollection = database.GetCollection<OfflineMailbox>(CollectionName);
        }

        public async Task<bool> SaveOfflineMessage(ChatMessage message, string to)
        {
            try
            {
                var filter = Builders<OfflineMailbox>.Filter.Eq(x => x.AccountUUid, to);
                var findResult = await _offlineMessageCollection.Find(filter).Project(x => x.MaxMessageId).Limit(1).ToListAsync();

                if(findResult.Count < 1)
                {
                    // mailbox does not exist， insert
                    var mailbox = new OfflineMailbox
                    {
                        AccountUUid = to,
                        LastUpdate = DateTime.UtcNow,
                        MaxMessageId = 1,
                        StartMessageId = 0,
                        Messages = new List<ChatMessage>()
                    };
                    message.MessageId = 0;
                    mailbox.Messages.Add(message);
                    try
                    {
                        await _offlineMessageCollection.InsertOneAsync(mailbox);
                    }
                    catch(MongoDuplicateKeyException e)
                    {
                        // temp fix for potential duplicate key problem
                        findResult = await _offlineMessageCollection.Find(filter).Project(x => x.MaxMessageId).Limit(1).ToListAsync();
                        message.MessageId = findResult.FirstOrDefault();
                        var updater = Builders<OfflineMailbox>.Update
                            .Push("Messages", message)
                            .Set("LastUpdate", DateTime.UtcNow)
                            .Inc("MaxMessageId", 1);
                        var updateResult = await _offlineMessageCollection.UpdateOneAsync(filter, updater);
                        return updateResult.IsAcknowledged;
                    }
                    
                    return true;
                }
                else
                {
                    // TODO: Time this section
                    var start_time = DateTime.UtcNow;
                    message.MessageId = findResult.FirstOrDefault();
                    var updater = Builders<OfflineMailbox>.Update
                    .Push("Messages", message)
                    .Set("LastUpdate", DateTime.UtcNow)
                    .Inc("MaxMessageId", 1);
                    var updateResult = await _offlineMessageCollection.UpdateOneAsync(filter, updater);
                    return updateResult.IsAcknowledged;
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public List<ChatMessage> GetAllOfflineMessages(string userName)
        {
            var filter = Builders<OfflineMailbox>.Filter.Eq(x => x.AccountUUid, userName);
            var findResult = _offlineMessageCollection.Find(filter).Limit(1).Project(x => x.Messages.Where(x => !x.Delivered)).FirstOrDefault().ToList();
            return findResult;
        }

        public List<ChatMessage> GetOfflineMessages(string userName, long offset, int size = 10)
        {
            var filter = Builders<OfflineMailbox>.Filter.Eq(x => x.AccountUUid, userName);
            var findResult = _offlineMessageCollection.Find(filter).Limit(1).Project(x => x.Messages.Where(x => !x.Delivered && x.MessageId >= offset)).FirstOrDefault().ToList().Take(size).ToList();
            return findResult;
        }

        public ChatMessage GetLastOfflineMessages(string userName)
        {
            var filter = Builders<OfflineMailbox>.Filter.Eq(x => x.AccountUUid, userName);
            var findResult = _offlineMessageCollection.Find(filter).Limit(1).Project(x => x.Messages.Last()).FirstOrDefault();
            return findResult;
        }

        public async void ACKMessages(string userName, long ExpectMessageId)
        {
            var filter = Builders<OfflineMailbox>.Filter;
            var messageFilter = filter.And(
                filter.Eq(x => x.AccountUUid, userName), 
                filter.ElemMatch(x => x.Messages, msg => msg.MessageId < ExpectMessageId && !msg.Delivered));
            var updater = Builders<OfflineMailbox>.Update.Set("Messages.$.Deliverd", true);
            await _offlineMessageCollection.UpdateOneAsync(messageFilter, updater);
        }
    }
}
