using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Model
{
    public abstract class ActivityDetailModel : ModelBase
    {
        public string Content { get; set; }
        public UserListModel Author { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
