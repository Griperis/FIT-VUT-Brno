using System;
using System.Collections.Generic;
using System.Text;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.DAL.Tests;
using Xunit;
using Xunit.Abstractions;

namespace TeamChat.BL.Tests
{
    public class IntegrationTest
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ITeamRepository _teamRepository;

        public IntegrationTest(ITestOutputHelper output)
        {
            ITeamChatDbContextFactory factory = new InMemoryDbContextFactory();
            this._userRepository = new UserRepository(factory);
            this._commentRepository = new CommentRepository(factory);
            this._postRepository = new PostRepository(factory);
            this._teamRepository = new TeamRepository(factory);
        }

        [Fact]
        public void AddCommentToPostInTeam_Integration_Test()
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
            var userTwoModel = _userRepository.Create(new UserDetailModel
            {
                Name = "User Two",
                Email = "user@two.test",
                Password = "pwd"
            });
            teamModel = _teamRepository.AddMember(teamModel, userOneModel);
            teamModel = _teamRepository.AddMember(teamModel, userTwoModel);
            
            // Update users teams
            userOneModel = _userRepository.GetById(userOneModel.Id);
            userTwoModel = _userRepository.GetById(userTwoModel.Id);

            // Post creation
            var postModelOne = _postRepository.Create(new PostDetailModel
                {
                    Content = "ContentOfPostOne",
                    Title = "TitleOfPostOne"
                },
                userOneModel);

            teamModel = _teamRepository.AddPost(teamModel, postModelOne);

            // Comment creation
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
                userTwoModel, 
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentTwo);

            var commentThree = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "3rdCommentOfPostTwo"
                },
                userOneModel,
                postModelOne);

            postModelOne = _postRepository.AddComment(postModelOne, commentThree);

            var postModelTwo = _postRepository.Create(new PostDetailModel
                {
                    Content = "ContentOfPostTwo",
                    Title = "TitleOfPostTwo"
                }, 
                userTwoModel);

            // Updated team model with comments
            teamModel = _teamRepository.GetById(teamModel.Id);

            // Updated user with activities
            userOneModel = _userRepository.GetById(userOneModel.Id);


            Assert.Equal(3, postModelOne.Comments.Count);
            Assert.Equal(3, userOneModel.Activities.Count);
            postModelOne = _postRepository.RemoveComment(postModelOne, commentOne);
            _commentRepository.Delete(commentOne);
            Assert.Equal(2, postModelOne.Comments.Count);

            userOneModel = _userRepository.GetById(userOneModel.Id);
            Assert.Equal(2, userOneModel.Activities.Count);


            _commentRepository.Delete(commentTwo);
            _commentRepository.Delete(commentThree);
            _postRepository.Delete(postModelOne);

            userOneModel = _userRepository.GetById(userOneModel.Id);
            Assert.Equal(0, userOneModel.Activities.Count);

            teamModel = _teamRepository.RemoveMember(teamModel, userOneModel);
            Assert.Equal(1, teamModel.Members.Count);

            userOneModel = _userRepository.GetById(userOneModel.Id);
            Assert.Equal(0, userOneModel.Teams.Count);

        }
        [Fact]
        public void RemoveUserWithPostAndComment_Integration_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel()
            {
                Name = "Tester",
                Email = "Test@test.cz",
                Password = "Aloha"
            });

            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "Test Team"
            });

            var postModel = _postRepository.Create(new PostDetailModel()
                {
                    Content = "Prispevek",
                    Title = "Titulek"
                },
                userModel);

            var commentModel = _commentRepository.Create(new CommentDetailModel
                {
                    Content = "Komentar"
                },  
                userModel,
                postModel);

            teamModel = _teamRepository.AddMember(teamModel, userModel);
            teamModel = _teamRepository.AddPost(teamModel, postModel);
            
            userModel = _userRepository.GetById(userModel.Id);

            teamModel = _teamRepository.RemoveMember(teamModel, userModel);

            Assert.Equal(0, teamModel.Members.Count);
            Assert.Equal(1, teamModel.Posts.Count);
            Assert.Equal(2, userModel.Activities.Count);

        }
        [Fact]
        public void RemoveTeamWithPost_Integration_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel()
            {
                Name = "Tester",
                Email = "Test@test.cz",
                Password = "Aloha"
            });

            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "Test Team"
            });

            var postModel = _postRepository.Create(new PostDetailModel()
                {
                    Content = "Prispevek",
                    Title = "Titulek"
                },
                userModel);

            teamModel = _teamRepository.AddMember(teamModel, userModel);
            teamModel = _teamRepository.AddPost(teamModel, postModel);
            foreach (var post in teamModel.Posts)
            {
                foreach (var comment in post.Comments)
                {
                    postModel = _postRepository.RemoveComment(post, comment);
                    _commentRepository.Delete(comment);
                }
                teamModel = _teamRepository.RemovePost(teamModel, postModel);
                _postRepository.Delete(post);
            }

            userModel = _userRepository.GetById(userModel.Id);

            _teamRepository.Delete(teamModel);

            Assert.Empty(_userRepository.GetActivitiesById(userModel.Id));



        }


    }
}
