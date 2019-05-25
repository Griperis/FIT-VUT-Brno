using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Messages
{
    public class UserLogedInMessage : IMessage
    {
        public Guid UserId { get; set; }
    }
}
