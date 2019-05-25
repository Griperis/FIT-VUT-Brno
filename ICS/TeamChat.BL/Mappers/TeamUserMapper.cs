using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{
    internal static class TeamUserMapper
    {
        public static TeamUser MapToEntity(TeamListModel teamListModel, UserListModel userListModel)
        {
            return new TeamUser
            {
                Team = TeamMapper.MapToEntity(TeamMapper.ListToDetailModel(teamListModel)),
                TeamId = teamListModel.Id,
                User = UserMapper.MapToEntity(UserMapper.ListToDetailModel(userListModel)),
                UserId = userListModel.Id

            };
        }
    }
}
