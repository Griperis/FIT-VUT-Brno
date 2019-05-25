using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Messages
{
    public class TeamDeletedMessage : IMessage
    {
        public Guid teamId;
    }
}
