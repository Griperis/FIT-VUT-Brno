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
    public class UserRepositoryTest
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ITeamRepository _teamRepository;

        public UserRepositoryTest(ITestOutputHelper output)
        {
            this._userRepository = new UserRepository(new InMemoryDbContextFactory());
            this._postRepository = new PostRepository(new InMemoryDbContextFactory());
            this._commentRepository = new CommentRepository(new InMemoryDbContextFactory());
            this._teamRepository = new TeamRepository(new InMemoryDbContextFactory());
        }

        [Fact]
        public void CreateUserModelTest()
        {
            //Arrange
            var model = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword" 
            };

            //Act
            var returnedModel = _userRepository.Create(model);

            //Assert
            Assert.NotNull(returnedModel);

            //Teardown
            _userRepository.Delete(returnedModel.Id);
        }

        [Fact]
        public void GetUserUserModelTest()
        {
            //Arrange
            var model = new UserDetailModel()
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };
            //Act
            var createdModel = _userRepository.Create(model);
            var byIdModel = _userRepository.GetById(createdModel.Id);

            //Assert
            Assert.NotNull(byIdModel);
           
            //Teardown
            _userRepository.Delete(createdModel.Id);
        }


        [Fact]
        public void GetUserByEmailUserModelTest()
        {
            //Arrange
            var model = new UserDetailModel
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };
            //Act
            var createdModel = _userRepository.Create(model);
            var byEmailModel = _userRepository.GetByEmail(createdModel.Email);

            //Assert
            Assert.NotNull(byEmailModel);

            //Teardown
            _userRepository.Delete(createdModel.Id);
        }

        [Fact]
        public void UpdateUserModelTest()
        {
            //Arrange
            var model = new UserDetailModel
            {
                Name = "NewUser",
                Email = "NewUser@email.cz",
                Password = "NewUserPassword"
            };
            //Act
            var createdModel = _userRepository.Create(model);
            var newModel = new UserDetailModel
            {
                Id = createdModel.Id,
                Name = "New---NewUser",
                Email = "New---NewUser@email.cz",
                Password = "New---NewUserPassword"
            };
            _userRepository.Update(newModel);
            var updatedModel = _userRepository.GetById(createdModel.Id);
           
            //Assert
            Assert.Equal("New---NewUser", updatedModel.Name);

            //Teardown
            _userRepository.Delete(createdModel.Id);
        }
        [Fact]
        public void GetActivitiesByIdTest()
        {
            //Arrange
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "TestTeam"
            });

            var userOneModel = _userRepository.Create(new UserDetailModel
            {
                Name = "User One",
                Email = "user@one.test",
                Password = "pwd"
            });

            var postModel = _postRepository.Create(new PostDetailModel
                {
                    Content = "ContentOfPostOne",
                    Title = "TitleOfPostOne"
                },
                userOneModel);

            teamModel = _teamRepository.AddPost(teamModel, postModel);

            var commentOne = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "CommentOfPostOne"
                },
                userOneModel,
                postModel);

            postModel = _postRepository.AddComment(postModel, commentOne);
            //Act
            var activities = _userRepository.GetActivitiesById(userOneModel.Id);

            //Assert
            Assert.NotNull(activities);
            //Teardown
            if (commentOne != null)
                _commentRepository.Delete(commentOne.Id);
            if (postModel != null)
                _postRepository.Delete(postModel.Id);
            if (userOneModel != null)
                _userRepository.Delete(userOneModel.Id);
            if (teamModel != null)
                _teamRepository.Delete(teamModel.Id);
        }

        [Fact]
        public void CheckActivitiesTest()
        {
           
            // Arrange
            // Team creation
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "TestTeam"
            });

            var userOneModel = _userRepository.Create(new UserDetailModel
            {
                Name = "User One",
                Email = "user@one.test",
                Password = "pwd"
            });

            teamModel = _teamRepository.AddMember(teamModel, userOneModel);
            
            // Update users teams
            userOneModel = _userRepository.GetById(userOneModel.Id);
     
            // Post creation
            var postModelOne = _postRepository.Create(new PostDetailModel
                {
                    Content = "ContentOfPostOne",
                    Title = "TitleOfPostOne"
                },
                userOneModel);

            teamModel = _teamRepository.AddPost(teamModel, postModelOne);

            // Comments creation
            var commentOne = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "CommentOfPostOne"
                }, 
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentOne);

            var commentTwo = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "2CommentOfPostTwo"
                }, 
                userOneModel, 
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentTwo);

            var commentThree = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "3rdCommentOfPostTwo"
                },
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentThree);


            // Updated team model with comments
            teamModel = _teamRepository.GetById(teamModel.Id);

            // Updated user with activities
            userOneModel = _userRepository.GetById(userOneModel.Id);

            //Assert
            Assert.Equal(4, userOneModel.Activities.Count);
            //Teardown
            if (commentOne != null)
                _commentRepository.Delete(commentOne.Id);
            if (commentTwo != null)
                _commentRepository.Delete(commentTwo.Id);
            if (commentThree != null)
                _commentRepository.Delete(commentThree.Id);
            if (postModelOne != null)
                _postRepository.Delete(postModelOne.Id);
            if (userOneModel != null)
                _userRepository.Delete(userOneModel.Id);
            if (teamModel != null)
                _teamRepository.Delete(teamModel.Id);
        }
        [Fact]
        public void CheckDeleteActivityTest()
        {

            // Arrange
            // Team creation
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "TestTeam"
            });

            var userOneModel = _userRepository.Create(new UserDetailModel
            {
                Name = "User One",
                Email = "user@one.test",
                Password = "pwd"
            });

            teamModel = _teamRepository.AddMember(teamModel, userOneModel);

            // Update users teams
            userOneModel = _userRepository.GetById(userOneModel.Id);

            // Post creation
            var postModelOne = _postRepository.Create(new PostDetailModel
            {
                Content = "ContentOfPostOne",
                Title = "TitleOfPostOne"
            },
                userOneModel);

            teamModel = _teamRepository.AddPost(teamModel, postModelOne);

            // Comments creation 3x
            var commentOne = _commentRepository.Create(new CommentDetailModel
            {
                Content = "CommentOfPostOne"
            },
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentOne);

            var commentTwo = _commentRepository.Create(new CommentDetailModel
            {
                Content = "2CommentOfPostTwo"
            },
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentTwo);

            var commentThree = _commentRepository.Create(new CommentDetailModel
            {
                Content = "3rdCommentOfPostTwo"
            },
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentThree);

            //Remove one comment
            postModelOne = _postRepository.RemoveComment(postModelOne, commentThree);
            _commentRepository.Delete(commentThree);
           
            // Updated team model with comments
            teamModel = _teamRepository.GetById(teamModel.Id);

            // Updated user with activities
            userOneModel = _userRepository.GetById(userOneModel.Id);

            //Assert

            Assert.Equal(3, userOneModel.Activities.Count);
            //Teardown
            if (commentOne != null)
                _commentRepository.Delete(commentOne.Id);
            if (commentTwo != null)
                _commentRepository.Delete(commentTwo.Id);
            if (postModelOne != null)
                _postRepository.Delete(postModelOne.Id);
            if (userOneModel != null)
                _userRepository.Delete(userOneModel.Id);
            if (teamModel != null)
                _teamRepository.Delete(teamModel.Id);
        }
    }
}
