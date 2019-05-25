using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Mappers;
using TeamChat.BL.Model;
using TeamChat.DAL;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ITeamChatDbContextFactory _dbContextFactory;

        public PostRepository(ITeamChatDbContextFactory dbContextFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }
        public PostDetailModel GetById(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return PostMapper.MapToDetailModel(dbContext.Posts
                    .Include(a => a.Author)
                    .Include(c => c.Comments)
                    .ThenInclude(ca => ca.Author)
                    .First(e => e.Id == id)
                );
            }
        }

        public PostDetailModel Create(PostDetailModel postModel, UserDetailModel authorModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                postModel.CreationTime = DateTime.Now;
                postModel.Author = UserMapper.DetailToListModel(authorModel);
                var postEntity = PostMapper.MapDetailModelToEntity(postModel);
                postEntity.Author = UserMapper.MapToEntity(authorModel);

                var userEntity = dbContext.Users
                    .First(u => u.Id == authorModel.Id);

                userEntity.Activities.Add(postEntity);
                dbContext.Users.Update(userEntity);
                dbContext.Posts.Update(postEntity);
                dbContext.SaveChanges();
                return PostMapper.MapToDetailModel(postEntity);
            }
        }

        public PostDetailModel UpdateTitle(PostDetailModel postModel, string title)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var postEntity = dbContext.Posts
                    .Include(a => a.Author)
                    .Include(c => c.Comments)
                    .ThenInclude(ca => ca.Author)
                    .First(p => p.Id == postModel.Id);
                postEntity.Title = title;
                dbContext.Posts.Update(postEntity);
                dbContext.SaveChanges();
                return PostMapper.MapToDetailModel(postEntity);
            }
        }

        public PostDetailModel UpdateContent(PostDetailModel postModel, string content)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var postEntity = dbContext.Posts
                    .Include(a => a.Author)
                    .Include(c => c.Comments)
                    .ThenInclude(c => c.Author)
                    .First(p => p.Id == postModel.Id);
                postEntity.Content = content;
                dbContext.Posts.Update(postEntity);
                dbContext.SaveChanges();
                return PostMapper.MapToDetailModel(postEntity);
            }
        }

        public IEnumerable<PostDetailModel> GetAllByTeamId(Guid teamId)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var team = dbContext.Teams
                    .Include(p => p.Posts)
                    .ThenInclude(c => c.Comments)
                    .ThenInclude(c => c.Author)
                    .Include(p => p.Posts)
                    .ThenInclude(a => a.Author)
                    .First(t => t.Id == teamId);

                return team.Posts.Select(PostMapper.MapToDetailModel).ToList();
            }
        }

        public PostDetailModel AddComment(PostDetailModel postModel, CommentDetailModel commentModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var postEntity = dbContext.Posts
                    .Include(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .Include(u => u.Author)
                    .First(p => p.Id == postModel.Id);

                dbContext.Posts.Update(postEntity);
                dbContext.SaveChanges();

                return PostMapper.MapToDetailModel(postEntity);
            }
        }

        public PostDetailModel RemoveComment(PostDetailModel postModel, CommentDetailModel commentModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var postEntity = dbContext.Posts
                    .Include(c => c.Comments)
                    .ThenInclude(a => a.Author)
                    .Include(a => a.Author)
                    .First(p => p.Id == postModel.Id);

                var commentEntity = dbContext.Comments
                    .Include(a => a.Author)
                    .First(c => c.Id == commentModel.Id);

                postEntity.Comments.Remove(commentEntity);
                commentEntity.BelongsTo = null;
                dbContext.Posts.Update(postEntity);
                dbContext.SaveChanges();

                return PostMapper.MapToDetailModel(postEntity);
            }
        }

        public void Delete(PostDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                RemovePostFromUserActivity(detailModel.Id, dbContext);
                dbContext.Remove(dbContext.Find(typeof(Post), detailModel.Id));
                dbContext.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                RemovePostFromUserActivity(id, dbContext);
                dbContext.Remove(dbContext.Find(typeof(Post), id));
                dbContext.SaveChanges();
            }
        }
        private void RemovePostFromUserActivity(Guid postId, TeamChatDbContext dbContext)
        {
            var postEntity = dbContext.Posts
                .Include(a => a.Author)
                .First(p => p.Id == postId);

            var userEntity = dbContext.Users
                .Include(a => a.Activities)
                .First(u => u.Id == postEntity.Author.Id);

            userEntity.Activities.Remove(postEntity);

            dbContext.Update(userEntity);

        }
    }
}
