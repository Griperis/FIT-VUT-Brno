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

namespace TeamChat.BL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ITeamChatDbContextFactory _dbContextFactory;

        public CommentRepository(ITeamChatDbContextFactory dbContextFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }
        public CommentDetailModel GetById(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                return CommentMapper.MapToDetailModel(dbContext.Comments
                    .Include(a => a.Author)
                    .Include(p => p.BelongsTo)
                    .ThenInclude(a => a.Author)
                    .First(e => e.Id == id));
            }
        }

        public CommentDetailModel Create(CommentDetailModel commentModel, UserDetailModel authorModel, PostDetailModel postModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                commentModel.BelongsTo = PostMapper.DetailToListModel(postModel);
                commentModel.Author = UserMapper.DetailToListModel(authorModel);
                commentModel.CreationTime = DateTime.Now;

                var userEntity = dbContext.Users
                    .Include(a => a.Activities)
                    .First(u => u.Id == commentModel.Author.Id);
                var commentEntity = CommentMapper.MapToEntity(commentModel);
                userEntity.Activities.Add(commentEntity);
                dbContext.Users.Update(userEntity);
                dbContext.Comments.Update(commentEntity);
                dbContext.SaveChanges();

                return CommentMapper.MapToDetailModel(commentEntity);
            }
        }

        public CommentDetailModel UpdateContent(CommentDetailModel commentModel, string content)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var commentEntity = dbContext.Comments
                    .Include(a => a.Author)
                    .Include(p => p.BelongsTo)
                    .ThenInclude(a => a.Author)
                    .First(c => c.Id == commentModel.Id);

                commentEntity.Content = content;
                dbContext.Comments.Update(commentEntity);
                dbContext.SaveChanges();
                return CommentMapper.MapToDetailModel(commentEntity);
            }
        }
        public IEnumerable<CommentDetailModel> GetAllByPostId(Guid postId)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                var post = dbContext.Posts
                    .Include(c => c.Comments)?
                    .ThenInclude(a => a.Author)
                    .First(p => p.Id == postId);

                return post.Comments.Select(CommentMapper.MapToDetailModel).ToList();
            }
        }

        public void Delete(CommentDetailModel detailModel)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {
                RemoveCommentFromUserActivity(detailModel.Id, dbContext);
                dbContext.Remove(dbContext.Find(typeof(Comment), detailModel.Id));
                dbContext.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var dbContext = _dbContextFactory.CreateTeamChatDbContext())
            {

                RemoveCommentFromUserActivity(id, dbContext);
                dbContext.Remove(dbContext.Find(typeof(Comment), id));
                dbContext.SaveChanges();
            }
        }
        private void RemoveCommentFromUserActivity(Guid commentId, TeamChatDbContext dbContext)
        {
            var commentEntity = dbContext.Comments
                .Include(a => a.Author)
                .First(c => c.Id == commentId);

            var userEntity = dbContext.Users
                .Include(a => a.Activities)
                .First(u => u.Id == commentEntity.Author.Id);

            userEntity.Activities.Remove(commentEntity);

            dbContext.Update(userEntity);
        }
    }
}
