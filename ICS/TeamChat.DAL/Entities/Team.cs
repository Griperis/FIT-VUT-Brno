using System.Collections.Generic;

namespace TeamChat.DAL.Entities
{
    public class Team : EntityBase
    {
        public string Name { get; set; }
        public ICollection<TeamUser> Members { get; set; } = new List<TeamUser>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
