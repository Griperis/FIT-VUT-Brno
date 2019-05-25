using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Messages
{
    public class CurrentUserIdMessage : IMessage
    {
        public Guid UserId { get; set; }
    }
}
