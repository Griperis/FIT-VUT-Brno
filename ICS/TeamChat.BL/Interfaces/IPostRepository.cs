using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Model;

namespace TeamChat.BL.Interfaces
{
    public interface IPostRepository
    {
        PostDetailModel GetById(Guid id);
        PostDetailModel Create(PostDetailModel postModel, UserDetailModel authorModel);
        PostDetailModel UpdateTitle(PostDetailModel postModel, string title);
        PostDetailModel UpdateContent(PostDetailModel postModel, string content);
        IEnumerable<PostDetailModel> GetAllByTeamId(Guid teamId);
        PostDetailModel AddComment(PostDetailModel postModel, CommentDetailModel commentModel);
        PostDetailModel RemoveComment(PostDetailModel postModel, CommentDetailModel commentModel);
        void Delete(PostDetailModel detailModel);
        void Delete(Guid id);
    }
}
