using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Model
{
    public class UserDetailModel : ModelBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public ICollection<ActivityDetailModel> Activities { get; set; } = new List<ActivityDetailModel>();
        public ICollection<TeamListModel> Teams { get; set; } = new List<TeamListModel>();
        public DateTime LastLoginTime { get; set; }
        
    }
}
