using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Model;

namespace TeamChat.BL.Messages
{
    public class PostsUpdatedMessage : IMessage
    {
        public PostDetailModel post;
    }
}
