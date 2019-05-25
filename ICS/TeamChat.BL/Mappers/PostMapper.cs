using System.Linq;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{
    internal static class PostMapper
    {
        public static Post MapDetailModelToEntity(PostDetailModel postDetailModel)
        {
            return new Post
            {
                Id = postDetailModel.Id,
                Title = postDetailModel.Title,
                Author = UserMapper.MapListModelToEntity(postDetailModel.Author),
                Content = postDetailModel.Content,
                CreationTime = postDetailModel.CreationTime,
                Comments = postDetailModel.Comments.Select(CommentMapper.MapToEntity).ToList()
            };
        }

        public static Post MapListModelToEntity(PostListModel postDetailModel)
        {
            return new Post
            {
                Id = postDetailModel.Id,
                Title = postDetailModel.Title,
                Author = UserMapper.MapListModelToEntity(postDetailModel.Author),
                Content = postDetailModel.Content,
                CreationTime = postDetailModel.CreationTime
            };
        }

        public static PostDetailModel MapToDetailModel(Post post)
        {
            var detailModel = new PostDetailModel
            {
                Id = post.Id,
                Title = post.Title,
                Author = UserMapper.MapToListModel(post.Author),
                Content = post.Content,
                CreationTime = post.CreationTime,
            };
            foreach (var comment in post.Comments)
            {
                detailModel.Comments.Add(CommentMapper.MapToDetailModel(comment));
            }
            return detailModel;

        }

        public static PostListModel MapToListModel(Post post)
        {
            return new PostListModel
            {
                Id = post.Id,
                Title = post.Title,
                Author = UserMapper.MapToListModel(post.Author),
                Content = post.Content,
                CreationTime = post.CreationTime
            };
        }

        public static PostListModel DetailToListModel(PostDetailModel postModel)
        {
            return new PostListModel
            {
                Id = postModel.Id,
                Author = postModel.Author,
                Content = postModel.Content,
                CreationTime = postModel.CreationTime,
                Title = postModel.Title
            };
        }
    }
}
