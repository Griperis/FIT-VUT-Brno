using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class TeamRepositoryTest
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;

        public TeamRepositoryTest(ITestOutputHelper output)
        {
            ITeamChatDbContextFactory factory = new InMemoryDbContextFactory();

            this._teamRepository = new TeamRepository(factory);
            this._userRepository = new UserRepository(factory);
            this._postRepository = new PostRepository(factory);
            this._commentRepository = new CommentRepository(factory);
        }

        [Fact]
        public void Create_TeamModel_Test()
        {
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "Muj tym",
            });

            Assert.NotNull(teamModel);

            _teamRepository.Delete(teamModel.Id);

        }

        [Fact]
        public void GetAll_TeamModel_Test()
        {
            for (int i = 0; i < 10; i++)
            {
                var teamModel = _teamRepository.Create(new TeamDetailModel()
                {
                    Name = i.ToString()
                });


            }

            IEnumerable<TeamListModel> returnedModels = _teamRepository.GetAll();
            Assert.NotNull(returnedModels);
            Assert.NotEmpty(returnedModels);

            foreach (var returnedModel in returnedModels)
            {
                _teamRepository.Delete(returnedModel.Id);
            }

        }

        [Fact]
        public void ChekTeamName_TeamModel_Test()
        {
            //Arrange
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "S tym"
            });

            //Assert
            Assert.Equal("S tym", teamModel.Name);

            //Teardown
            _teamRepository.Delete(teamModel.Id);
        }

        [Fact]
        public void Update_TeamModel_Test()
        {
            var teamOnemodel = new TeamDetailModel
            {
                Name = "Stary tym"
            };

            var teamTwoModel = new TeamDetailModel
            {
                Name = "Muj tym",
            };

            var returnedModel = _teamRepository.Create(teamOnemodel);

            returnedModel = _teamRepository.Create(teamTwoModel);

            var newModel = new TeamDetailModel
            {
                Id = returnedModel.Id,
                Name = "Novy tym",
            };

            _teamRepository.Create(newModel);

            var updatedModel = _teamRepository.GetById(returnedModel.Id);

            Assert.Equal("Novy tym", updatedModel.Name);

            //Teardown

            _teamRepository.Delete(updatedModel.Id);
        }

        [Fact]
        public void AddMember_TeamModel_Test()
        {
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "ICS Team",
            });

            var userOneModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Brisk",
                Email = "brisk@email.cz",
                Password = "briskunbreakableheslo",
            });

            var userTwoModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Franta",
                Email = "franta@email.cz",
                Password = "frantaunbreakableheslo",
            });


            teamModel = _teamRepository.AddMember(teamModel, userOneModel);
            teamModel = _teamRepository.AddMember(teamModel, userTwoModel);


            Assert.NotEmpty(teamModel.Members);
            Assert.Equal(2, teamModel.Members.Count);

           
            teamModel = _teamRepository.RemoveMember(teamModel, userOneModel);
            teamModel = _teamRepository.RemoveMember(teamModel, userTwoModel);

            //Teardown
            _userRepository.Delete(userOneModel.Id);
            _userRepository.Delete(userTwoModel.Id);

            _teamRepository.Delete(teamModel.Id);



        }

        [Fact]
        public void Add_Remove_Post_TeamModel_Test()
        {
            var teamOne = new TeamDetailModel
            {
                Name = "ICS Team",
            };

            var user = new UserDetailModel
            {
                Name = "Brisk",
                Email = "brisk@email.cz",
                Password = "briskunbreakableheslo",
            };


            var postOne = new PostDetailModel
            {
                Title = "Post v tymu",
                Content = "Jeste neni konec",
            };

            var postTwo = new PostDetailModel
            {
                Title = "Post v tymu",
                Content = "Uz je konec",
            };

            var userModel = _userRepository.Create(user);
            var postOneModel = _postRepository.Create(postOne, userModel);
            var postTwoModel = _postRepository.Create(postTwo, userModel);

            var teamModel = _teamRepository.Create(teamOne);


            Assert.NotNull(teamModel);
            Assert.Equal(teamOne.Name, teamModel.Name);

            teamModel = _teamRepository.AddPost(teamModel, postOneModel);

            Assert.Single(teamModel.Posts);

            teamModel = _teamRepository.AddPost(teamModel, postTwoModel);

            Assert.Equal(2, teamModel.Posts.Count);

            teamModel = _teamRepository.RemovePost(teamModel, postOneModel);

            Assert.Single(teamModel.Posts);

            //Teardown
            _userRepository.Delete(userModel.Id);
            _teamRepository.Delete(teamModel.Id);



        }

        [Fact]
        public void AddCommentToPost_TeamModel_Test()
        {
            var teamModel = _teamRepository.Create(new TeamDetailModel
            {
                Name = "ICS Team",
            });

            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Brisk",
                Email = "brisk@email.cz",
                Password = "briskunbreakableheslo",
            });

            var postOneModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Post v tymu",
                Content = "Jeste neni konec",
            }, userModel);

            var postTwoModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Druhy Post",
                Content = "Uz je konec",
                CreationTime = DateTime.Now,
            }, userModel);



            var commentModel = _commentRepository.Create(new CommentDetailModel
            {
                Content = "Komentar",
                CreationTime = DateTime.Now,
            }, userModel, postOneModel);


     
            teamModel = _teamRepository.AddPost(teamModel, postOneModel);
            teamModel = _teamRepository.AddPost(teamModel, postTwoModel);

           
            Assert.NotEmpty(teamModel.Posts.First().Comments);
            Assert.Equal(commentModel.Content, teamModel.Posts.First().Comments.First().Content);


            //Teardown
            _teamRepository.Delete(teamModel.Id);

        }



    }
}
