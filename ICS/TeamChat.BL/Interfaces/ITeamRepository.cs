using System;
using System.Collections.Generic;
using TeamChat.BL.Model;

namespace TeamChat.BL.Interfaces
{
    public interface ITeamRepository 
    {
        TeamDetailModel GetById(Guid id);
        TeamDetailModel Create(TeamDetailModel detailModel);
        TeamDetailModel UpdateName(Guid teamId, string newTeamName);
        IEnumerable<TeamListModel> GetAll();
        IEnumerable<TeamListModel> GetAllByUserId(Guid userId);
        TeamDetailModel AddMember(TeamDetailModel teamModel, UserDetailModel userModel);
        TeamDetailModel RemoveMember(TeamDetailModel teamModel, UserDetailModel userModel);
        TeamDetailModel AddPost(TeamDetailModel teamModel, PostDetailModel postModel);
        TeamDetailModel RemovePost(TeamDetailModel teamModel, PostDetailModel postModel);
        void Delete(Guid id);
        void Delete(TeamDetailModel detailModel);
    }
}