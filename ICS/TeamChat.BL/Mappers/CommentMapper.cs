using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{
    internal static class CommentMapper
    {
        public static Comment MapToEntity(CommentDetailModel commentDetailModel)
        {
            return new Comment
            {
                Id = commentDetailModel.Id,
                Author = UserMapper.MapListModelToEntity(commentDetailModel.Author),
                BelongsTo = PostMapper.MapListModelToEntity(commentDetailModel.BelongsTo),
                CreationTime = commentDetailModel.CreationTime,
                Content = commentDetailModel.Content
            };
        }

        public static CommentDetailModel MapToDetailModel(Comment comment)
        {
            return new CommentDetailModel
            {
                Id = comment.Id,
                Author = UserMapper.MapToListModel(comment.Author),
                BelongsTo = PostMapper.MapToListModel(comment.BelongsTo),
                CreationTime = comment.CreationTime,
                Content = comment.Content

            };
        }
    }
}
