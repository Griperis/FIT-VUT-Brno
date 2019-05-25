using System;
using System.Collections.Generic;

namespace TeamChat.DAL.Entities
{
    public abstract class Activity : EntityBase
    {
        public string Content { get; set; }
        public User Author { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
