using System;

namespace TeamChat.BL.Messages
{
    public class TeamSelectedMessage : IMessage
    {
        public Guid TeamId { get; set; }
    }
}