using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Clauses;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Mappers;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;
using TeamChat.DAL.Interfaces;

namespace TeamChat.BL.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ITeamChatDbContextFactory _dbContextFactory;

        public TeamRepository(ITeamChatDbContextFactory dbContextFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }

        public TeamDetailModel GetById(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return TeamMapper.MapToDetailModel(dbContext.Teams
                    .Include(tu => tu.Members)
                    .ThenInclude(u => u.User)
                    .Include(p => p.Posts)
                    .ThenInclude(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .First(t => t.Id == id));
            }
        }

        public TeamDetailModel Create(TeamDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var team = TeamMapper.MapToEntity(detailModel);
                dbContext.Teams.Update(team);
                dbContext.SaveChanges();
                return TeamMapper.MapToDetailModel(team);
            }
        }

        public TeamDetailModel UpdateName(Guid teamId, string newTeamName)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var teamEntity = dbContext.Teams
                    .Include(tu => tu.Members)
                    .ThenInclude(u => u.User)
                    .Include(p => p.Posts)
                    .ThenInclude(c => c.Comments)
                    .First(t => t.Id == teamId);

                teamEntity.Name = newTeamName;
                dbContext.Teams.Update(teamEntity);
                dbContext.SaveChanges();
                return TeamMapper.MapToDetailModel(teamEntity);
            }
        }

        public IEnumerable<TeamListModel> GetAll()
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return dbContext.Teams
                    .Select(TeamMapper.MapToListModel)
                    .ToList();
            }
        }

        public IEnumerable<TeamListModel> GetAllByUserId(Guid userId)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var query = (from team in dbContext.Teams
                             where team.Members.Any(u => u.UserId == userId)
                             select team).AsEnumerable();
                return query.Select(TeamMapper.MapToListModel).ToList();
            }
        }

        public TeamDetailModel AddMember(TeamDetailModel teamModel, UserDetailModel userModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var userEntity = dbContext.Users
                    .Include(tu => tu.Teams)
                    .ThenInclude(u => u.User)
                    .Include(tu => tu.Teams)
                    .ThenInclude(t => t.Team)
                    .Include(u => u.Activities)
                    .First(u => u.Id == userModel.Id);

                var teamEntity = dbContext.Teams
                    .Include(tu => tu.Members)
                    .ThenInclude(t => t.Team)
                    .Include(tu => tu.Members)
                    .ThenInclude(u => u.User)
                    .Include(t => t.Posts)
                    .ThenInclude(c => c.Comments)
                    .First(t => t.Id == teamModel.Id);

                var junction = new TeamUser
                {
                    Team = teamEntity,
                    TeamId = teamEntity.Id,
                    User = userEntity,
                    UserId = userEntity.Id
                };

                userEntity.Teams.Add(junction);
                teamEntity.Members.Add(junction);
                dbContext.TeamUsers.Add(junction);
                dbContext.Users.Update(userEntity);
                dbContext.Teams.Update(teamEntity);
                dbContext.SaveChanges();

                return TeamMapper.MapToDetailModel(teamEntity);
            }
        }

        public TeamDetailModel RemoveMember(TeamDetailModel teamModel, UserDetailModel userModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var teamEntity = dbContext.Teams
                    .Include(tu => tu.Members)
                    .ThenInclude(t => t.Team)
                    .Include(tu => tu.Members)
                    .ThenInclude(u => u.User)
                    .Include(t => t.Posts)
                    .ThenInclude(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .First(t => t.Id == teamModel.Id);

                var junction = dbContext.TeamUsers
                    .Where(t => t.TeamId == teamModel.Id)
                    .First(t => t.UserId == userModel.Id);

                var userEntity = dbContext.Users
                    .Include(tu => tu.Teams)
                    .ThenInclude(u => u.User)
                    .Include(tu => tu.Teams)
                    .ThenInclude(t => t.Team)
                    .Include(u => u.Activities)
                    .First(u => u.Id == userModel.Id);

                teamEntity.Members.Remove(junction);
                userEntity.Teams.Remove(junction);
                dbContext.TeamUsers.Remove(junction);

                dbContext.Users.Update(userEntity);
                dbContext.Teams.Update(teamEntity);

                dbContext.SaveChanges();
                return TeamMapper.MapToDetailModel(teamEntity);
            }

        }
        public TeamDetailModel AddPost(TeamDetailModel teamModel, PostDetailModel postModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var teamEntity = dbContext.Teams
                    .Include(p => p.Posts)?
                    .ThenInclude(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .Include(m => m.Members)
                    .ThenInclude(u => u.User)
                    .First(t => t.Id == teamModel.Id);

                var postEntity = dbContext.Posts
                    .Include(a => a.Author)
                    .Include(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .First(p => p.Id == postModel.Id);

                teamEntity.Posts.Add(postEntity);
                dbContext.Teams.Update(teamEntity);
                dbContext.SaveChanges();

                return TeamMapper.MapToDetailModel(teamEntity);
            }
        }

        public TeamDetailModel RemovePost(TeamDetailModel teamModel, PostDetailModel postModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var teamEntity = dbContext.Teams
                    .Include(p => p.Posts)?
                    .ThenInclude(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .Include(m => m.Members)
                    .ThenInclude(u => u.User)
                    .First(t => t.Id == teamModel.Id);


                var postEntity = dbContext.Posts
                    .Include(a => a.Author)
                    .Include(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .First(p => p.Id == postModel.Id);

                teamEntity.Posts.Remove(postEntity);
                dbContext.SaveChanges();

                return TeamMapper.MapToDetailModel(teamEntity);
            }
        }

        public void Delete(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                dbContext.Remove(dbContext.Find(typeof(Team), id));
                dbContext.SaveChanges();
            }
        }

        public void Delete(TeamDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                dbContext.Remove(dbContext.Find(typeof(Team), detailModel.Id));
                dbContext.SaveChanges();
            }
        }
    }

}
