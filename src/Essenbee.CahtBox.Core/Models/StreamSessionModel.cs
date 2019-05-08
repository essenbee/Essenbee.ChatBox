using System;

namespace Essenbee.ChatBox.Core.Models
{
    public class StreamSessionModel
    {
        public int Id { get; set; }
        public string ChannelId { get; set; }
        public DateTime UtcStartTime { get; set; }
        public DateTime UtcEndTime { get; set; }
        public DateTime LocalStartTime { get; set; }
        public DateTime LocalEndTime { get; set; }
    }
}