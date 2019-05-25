using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Messages
{
   public class TeamOpenDetailMessage : IMessage
    {
        public Guid TeamId { get; set; }
    }
}
