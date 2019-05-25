using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Messages
{
    public class CommentsUpdatedMessage : IMessage
    {
        public Guid postId;
    }
}
