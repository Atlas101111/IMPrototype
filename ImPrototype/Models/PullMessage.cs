using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImPrototype.Models
{
    public class PullOfflineMessageRequest
    {
        public string AccountUuid { get; set; }
        public string AccessKey { get; set; }
        public string RequestId { get; set; }
        public long Offset { get; set; }        // 起始位置，为0时
        public int Size { get; set; }          // 拉取数量
    }

    public class PullOfflineMessageResponse
    {
        public List<ChatMessage> ChatMessages;
    }

    public class PullHistoryMessageRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public long Size { get; set; }          // 向后拉取数量
        public DateTime StartTime { get; set; }     // 起始时间
    }

    public class PullHistoryMessageResponse
    {
        public List<ChatMessage> ChatMessages;
    }
}
