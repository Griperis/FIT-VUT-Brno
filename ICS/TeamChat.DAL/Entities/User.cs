using System;
using System.Collections.Generic;

namespace TeamChat.DAL.Entities
{
    public class User : EntityBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<TeamUser> Teams { get; set; } = new List<TeamUser>();
        public DateTime LastLoginTime { get; set; }

    }
}
