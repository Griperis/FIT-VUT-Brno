using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Model
{
    public class TeamDetailModel : ModelBase
    {
        public string Name { get; set; }
        public ICollection<UserListModel> Members { get; set; } = new List<UserListModel>();
        public ICollection<PostDetailModel> Posts { get; set; } = new List<PostDetailModel>();
    }
}
