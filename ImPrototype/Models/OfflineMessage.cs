using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImPrototype.Models
{
    public class OfflineMailbox
    {
        public string AccountUUid { get; set; }
        public long StartMessageId { get; set; }
        public long MaxMessageId { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatMessage
    {
        public long MessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = string.Empty;
        public bool Delivered { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }

    public enum MessageType
    {
        Text = 1,
        Audio = 2,
        Video = 3,
        Event = 4
    }
}
