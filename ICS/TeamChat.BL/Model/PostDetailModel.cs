using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamChat.BL.Model
{
    public class PostDetailModel : ActivityDetailModel, IComparable<PostDetailModel>
    {
        public string Title { get; set; }
        public ICollection<CommentDetailModel> Comments { get; set; } = new List<CommentDetailModel>();

        public int CompareTo(PostDetailModel other)
        {
            if (this.Comments.Count > 0 && other.Comments.Count > 0)
            {
               return -(DateTime.Compare(this.Comments.ElementAt(this.Comments.Count - 1).CreationTime, other.Comments.ElementAt(other.Comments.Count - 1).CreationTime));
            }

            return 0;

        }

        private List<CommentDetailModel> SortComments(ICollection<CommentDetailModel> comments)
        {
            List<CommentDetailModel> ListComments = new List<CommentDetailModel>(comments);
            ListComments.Sort();
            return ListComments;
        }


    }
}
