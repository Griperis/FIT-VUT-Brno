using System;
using System.Collections.Generic;

namespace TeamChat.DAL.Entities
{
    public class Post : Activity
    {
        public string Title { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}