using System;
using System.Collections.Generic;
using System.Linq;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.DAL.Entities;
using TeamChat.DAL.Interfaces;
using TeamChat.DAL.Tests;
using Xunit;
using Xunit.Abstractions;

namespace TeamChat.BL.Tests
{
    public class CommentRepositoryTest
    {
        private readonly CommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly PostRepository _postRepository;

        public CommentRepositoryTest(ITestOutputHelper output)
        {
            ITeamChatDbContextFactory factory = new InMemoryDbContextFactory();
            this._userRepository = new UserRepository(factory);
            this._commentRepository = new CommentRepository(factory);        
            this._postRepository = new PostRepository(factory);
        }

        [Fact]
        public void CreateCommentModelTest()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };

            var postListModel = new PostListModel()
            {
                Title = "NewTitle"
            };

            var commentModel = new CommentDetailModel()
            {
                CreationTime = new DateTime(),
                BelongsTo = postListModel,
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);

            //Assert
            Assert.NotNull(returnedCommentDetailModel);

            //Teardown           
            _commentRepository.Delete(returnedCommentDetailModel);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }
        [Fact]
        public void CreateCommentModelTest2()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };
            var postListModel = new PostListModel()
            {
                Title = "NewTitle"
            };

            var commentModel = new CommentDetailModel()
            {
                CreationTime = new DateTime(),
                BelongsTo = postListModel,
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);

            //Assert
            Assert.Equal("NewTitle", returnedCommentDetailModel.BelongsTo.Title);

            //Teardown
            _commentRepository.Delete(returnedCommentDetailModel);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }
        [Fact]
        public void GetByIdCommentModelTest()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };
            var date = new DateTime();

            var commentModel = new CommentDetailModel()
            {
                CreationTime = date,
                BelongsTo = new PostListModel(),
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);
            var returnedGetById = _commentRepository.GetById(returnedCommentDetailModel.Id);

            //Assert
            Assert.NotNull(returnedGetById);

            //Teardown
            _commentRepository.Delete(returnedGetById);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }

        [Fact]
        public void GetByIdCommentModelTest2()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };
            var date = DateTime.Now;

            var commentModel = new CommentDetailModel()
            {
                CreationTime = date,
                BelongsTo = new PostListModel(),
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);
            var returnedGetById = _commentRepository.GetById(returnedCommentDetailModel.Id);

            //Assert
            Assert.Equal(date.Date, returnedGetById.CreationTime.Date);

            //Teardown
            _commentRepository.Delete(returnedGetById);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }
        [Fact]
        public void GetByIdCommentModelTest3()
        {
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };
            //Arrange
            var date = new DateTime();
            var postListModel = new PostListModel()
            {
                Title = "NewTitle"
            };

            var commentModel = new CommentDetailModel()
            {
                CreationTime = date,
                BelongsTo = postListModel,
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);
            var returnedGetById = _commentRepository.GetById(returnedCommentDetailModel.Id);

            //Assert
            Assert.Equal("NewTitle", returnedGetById.BelongsTo.Title);

            //Teardown
            _commentRepository.Delete(returnedGetById);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }


        [Fact]
        public void UpdateCommentContent_Test()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
            };
            var commentModelOld = new CommentDetailModel()
            {
                CreationTime = new DateTime(),
                Content = "OldContent"
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModelOld, returnedUserModel, returnedPostDetailModel);
            returnedPostDetailModel = _postRepository.AddComment(returnedPostDetailModel, returnedCommentDetailModel);
            var returnedUpdatedComment = _commentRepository.UpdateContent(returnedCommentDetailModel, "NewContent");

            //Assert
            Assert.Equal("NewContent", returnedUpdatedComment.Content);

            //Teardown
            _commentRepository.Delete(returnedCommentDetailModel);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }
        [Fact]
        public void GetAllByPostId_CommentModel_Test()
        {
            //Arrange
            var postDetailModel = new PostDetailModel()
            {
                Title = "NewTitle",
                Comments = new List<CommentDetailModel>(5)
            };
            var date = new DateTime();
            var postListModel = new PostListModel()
            {
                Title = "NewTitle"
            };
            var commentModel = new CommentDetailModel()
            {
                CreationTime = date,
                BelongsTo = postListModel,
            };
            var userDetailModel = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };

            //Act
            var returnedUserModel = _userRepository.Create(userDetailModel);
            var returnedPostDetailModel = _postRepository.Create(postDetailModel, returnedUserModel);
            var returnedCommentDetailModel = _commentRepository.Create(commentModel, returnedUserModel, returnedPostDetailModel);
            returnedPostDetailModel = _postRepository.AddComment(returnedPostDetailModel, returnedCommentDetailModel);
            var returnedGetAllByPostId = _commentRepository.GetAllByPostId(returnedPostDetailModel.Id);

            //Assert
            Assert.NotNull(returnedGetAllByPostId);

            //Teardown

            _commentRepository.Delete(returnedCommentDetailModel);
            _postRepository.Delete(returnedPostDetailModel);
            _userRepository.Delete(returnedUserModel);
        }



    }
}
