using System;
using System.Collections.Generic;
using System.Text;

namespace TeamChat.BL.Model
{
    public class CommentDetailModel : ActivityDetailModel, IComparable<CommentDetailModel>
    {
        public PostListModel BelongsTo { get; set; }
        public int CompareTo(CommentDetailModel other)
        {
            return DateTime.Compare(this.CreationTime, other.CreationTime);
        }
    }
}
