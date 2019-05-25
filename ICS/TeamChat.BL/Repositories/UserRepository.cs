using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Mappers;
using TeamChat.BL.Model;
using TeamChat.DAL;
using TeamChat.DAL.Entities;
using TeamChat.DAL.Interfaces;

namespace TeamChat.BL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ITeamChatDbContextFactory _dbContextFactory;
        public UserRepository(ITeamChatDbContextFactory dbContextFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }

        public UserDetailModel Create(UserDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var user = UserMapper.MapWithPasswordHashToEntity(detailModel);
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
                return UserMapper.MapToDetailModel(user);
            }
        }

        public UserDetailModel Update(UserDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var user = UserMapper.MapToEntity(detailModel);
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
                return UserMapper.MapToDetailModel(user);
            }
        }
        public IEnumerable<UserListModel> GetAll()
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return dbContext.Users
                    .Select(UserMapper.MapToListModel)
                    .ToList();
            }
        }

        public UserDetailModel GetByEmail(string email)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return UserMapper.MapToDetailModel(dbContext.Users.First(e => e.Email == email));
            }
        }

        public UserDetailModel GetById(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            { 
                var userEntity = dbContext.Users
                    .Include(t => t.Teams)
                    .ThenInclude(t => t.Team)
                    .Include(a => a.Activities)
                    .ThenInclude(a => a.Author)
                    .First(e => e.Id == id);

                userEntity.Activities = GetCommentsWithDetails(userEntity, dbContext);

                return UserMapper.MapToDetailModel(userEntity);
            }
        }

        private static List<Activity> GetCommentsWithDetails(User userEntity, TeamChatDbContext dbContext)
        {
            var activities = new List<Activity>();
            foreach (var activity in userEntity.Activities)
            {
                if (activity is Comment commentActivity)
                {
                    var comment = dbContext.Comments
                        .Include(p => p.BelongsTo)
                        .ThenInclude(a => a.Author)
                        .First(e => e.Id == commentActivity.Id);
                    commentActivity.BelongsTo = comment.BelongsTo;
                    activities.Add(commentActivity);
                }
                else
                {
                    activities.Add(activity);
                }
            }

            return activities;
        }

        public IEnumerable<UserListModel> GetAllByTeamId(Guid teamId)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var query = (from user in dbContext.Users
                            where user.Teams.Any(e => e.TeamId == teamId)
                            select user).AsEnumerable();

                return query.Select(UserMapper.MapToListModel).ToList();
            }
        }

        public IEnumerable<ActivityDetailModel> GetActivitiesById(Guid userId)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var user = dbContext.Users
                    .Include(a => a.Activities)
                    .ThenInclude(a => a.Author)
                    .First(u => u.Id == userId);
                user.Activities = GetCommentsWithDetails(user, dbContext);

                return user.Activities.Select(ActivityMapper.MapToDetailModel).ToList();
            }
        }

        public void Delete(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                dbContext.Remove(dbContext.Find(typeof(User), id));
                dbContext.SaveChanges();
            }
        }
        public void Delete(UserDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                dbContext.Remove(dbContext.Find(typeof(User), detailModel.Id));
                dbContext.SaveChanges();
            }
        }
        public void SetUserLoginTime(UserDetailModel userDetailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var userEntity = dbContext.Users.First(u => u.Id == userDetailModel.Id);
                userEntity.LastLoginTime = DateTime.Now;
                dbContext.Users.Update(userEntity);
                dbContext.SaveChanges();
            }
        }
    }
}
