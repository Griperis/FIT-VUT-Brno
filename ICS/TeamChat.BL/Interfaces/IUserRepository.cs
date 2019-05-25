using System;
using System.Collections.Generic;
using TeamChat.BL.Model;

namespace TeamChat.BL.Interfaces
{
    public interface IUserRepository
    {
        UserDetailModel GetById(Guid id);
        UserDetailModel GetByEmail(string email);
        UserDetailModel Update(UserDetailModel detailModel);
        UserDetailModel Create(UserDetailModel detailModel);
        IEnumerable<UserListModel> GetAll();
        IEnumerable<UserListModel> GetAllByTeamId(Guid teamId);
        IEnumerable<ActivityDetailModel> GetActivitiesById(Guid id);
        void SetUserLoginTime(UserDetailModel detailModel);
        void Delete(Guid id);
        void Delete(UserDetailModel detailModel);


    }
}