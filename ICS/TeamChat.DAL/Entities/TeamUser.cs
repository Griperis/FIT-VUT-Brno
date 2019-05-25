using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.DAL.Entities
{
    public class TeamUser : EntityBase
    {
        public Team Team { get; set; }
        public Guid TeamId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}
