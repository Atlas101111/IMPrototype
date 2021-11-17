using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImPrototype.Models
{
    public class PushMessageRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string GroupId { get; set; }
        public string Content { get; set; }
        public string AccessKey { get; set; }
        public int DelayMilliseconds { get; set; }
        public MessageChannel Channel { get; set; }
        public MessageType Type { get; set; }
    }

    public class PushMessageResponse
    {
        public bool Success { get; set; }
    }

    public class ACKMessageRequest
    {
        public string AccountUuid { get; set; }
        public long ExpectMessageId { get; set; }
    }

    public enum MessageChannel
    {
        XEva = 1,
        Polome = 2,
        Island = 3
    }

}
