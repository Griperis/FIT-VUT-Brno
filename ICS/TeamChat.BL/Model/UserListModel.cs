using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Model
{
    public class UserListModel : ModelBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public DateTime LastLoginTime { get; set; }

    }
}
