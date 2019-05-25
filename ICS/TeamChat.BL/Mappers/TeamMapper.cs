using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{
    internal static class TeamMapper
    {
        public static TeamDetailModel MapToDetailModel(Team team)
        {
            var teamDetailModel = new TeamDetailModel
            {
                Id = team.Id,
                Name = team.Name,
                Posts = team.Posts.Select(PostMapper.MapToDetailModel).ToList(),
            };
            foreach (var member in team.Members)
            {
                teamDetailModel.Members.Add(UserMapper.MapToListModel(member.User));
            }
            return teamDetailModel;
        }

        public static TeamListModel MapToListModel(Team team)
        {
            return new TeamListModel
            {
                Id = team.Id,
                Name = team.Name
            };
        }

        public static Team MapToEntity(TeamDetailModel detailModel)
        {
            var team = new Team
            {
                Id = detailModel.Id,
                Name = detailModel.Name,
                Posts = detailModel.Posts.Select(PostMapper.MapDetailModelToEntity).ToList(),
            };
            foreach (var member in detailModel.Members)
            {
                team.Members.Add(TeamUserMapper.MapToEntity(MapToListModel(team), member));
            }
            return team;
        }

        public static TeamDetailModel ListToDetailModel(TeamListModel listModel)
        {
            return new TeamDetailModel
            {
                Id = listModel.Id,
                Name = listModel.Name
            };
        }
    }
}
