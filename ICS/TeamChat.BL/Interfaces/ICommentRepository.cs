using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Model;

namespace TeamChat.BL.Interfaces
{
    public interface ICommentRepository
    {
        CommentDetailModel GetById(Guid id);
        CommentDetailModel Create(CommentDetailModel commentModel, UserDetailModel authorModel, PostDetailModel postModel);
        CommentDetailModel UpdateContent(CommentDetailModel commentModel, string content);
        IEnumerable<CommentDetailModel> GetAllByPostId(Guid postId);
        void Delete(CommentDetailModel detailModel);
        void Delete(Guid id);

    }
}
