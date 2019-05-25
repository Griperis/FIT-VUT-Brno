using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.DAL.Tests;
using Xunit;
using Xunit.Abstractions;

namespace TeamChat.BL.Tests
{
    public class PostRepositoryTest
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ITestOutputHelper _console;

        public PostRepositoryTest(ITestOutputHelper output)
        {
            ITeamChatDbContextFactory factory = new InMemoryDbContextFactory();
            this._postRepository = new PostRepository(factory);
            this._userRepository = new UserRepository(factory);
            this._teamRepository = new TeamRepository(factory);
            this._commentRepository = new CommentRepository(factory);
            this._console = output;
        }

        [Fact]
        public void Create_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Dano",
                Email = "drevo@seznam.cz",
                Password = "drevobezdrevanenidrevo",

            });


            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Post ",
                Content = "halabala",
            }, userModel);

            Assert.NotNull(postModel);
            Assert.Equal("Post ", postModel.Title);
            Assert.Equal("Dano", postModel.Author.Name);

            _postRepository.Delete(postModel.Id);

        }

        [Fact]
        public void UpdateTitle_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Dano",
                Email = "drevo@seznam.cz",
                Password = "drevobezdrevanenidrevo",

            });

            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Second Post",
            }, userModel);


            Assert.NotNull(postModel);
            Assert.Equal("Second Post", postModel.Title);

            postModel = _postRepository.UpdateTitle(postModel, "new Title");

            Assert.NotNull(postModel);
            Assert.Equal("new Title", postModel.Title);
            Assert.Equal("Dano", postModel.Author.Name);

            _postRepository.Delete(postModel.Id);
            _userRepository.Delete(userModel.Id);

        }

        [Fact]
        public void GetAllByTeamId_PostModelTest()
        {
            var postOneModel = new PostDetailModel
            {
                Title = "Test Post"
            };
            var postTwoModel = new PostDetailModel
            {
                Title = "Test Post 2"
            };

            var teamModel = new TeamDetailModel
            {
                Name = "Test Team",

            };

            var authorModel = new UserDetailModel
            {
                Name = "Test Author",
                Password = "TestPassword"
            };

            teamModel = _teamRepository.Create(teamModel);
            authorModel = _userRepository.Create(authorModel);
            postOneModel = _postRepository.Create(postOneModel, authorModel);
            postTwoModel = _postRepository.Create(postTwoModel, authorModel);

            teamModel = _teamRepository.AddPost(teamModel, postOneModel);
            teamModel = _teamRepository.AddPost(teamModel, postTwoModel);

            var posts = _postRepository.GetAllByTeamId(teamModel.Id);
            Assert.NotNull(posts);
            Assert.NotEmpty(posts);
            Assert.Equal(2, posts.Count());
            foreach (var post in posts)
            {
                _console.WriteLine(post.Title);
            }

            //Teardown
            _postRepository.Delete(postOneModel.Id);
            _postRepository.Delete(postTwoModel.Id);
            _userRepository.Delete(authorModel.Id);
            _teamRepository.Delete(teamModel.Id);
  


        }

        [Fact]
        public void GetById_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Dano",
                Email = "drevo@seznam.cz",
                Password = "drevobezdrevanenidrevo",

            });

            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Second Post",
            }, userModel);


            postModel = _postRepository.GetById(postModel.Id);

            Assert.NotNull(postModel);

            _postRepository.Delete(postModel.Id);
            _userRepository.Delete(userModel.Id);
        


        }
        [Fact]
        public void AddCommentBasic_PostModel_Test()
        {
            var userModel = new UserDetailModel
            {
                Name = "Dan",
                Email = "drevo@seznam.com",
                Password = "drevobezdreva",

            };
            var userTwoModel = new UserDetailModel
            {
                Name = "Tester",
                Email = "",
                Password = "",
            };
            var postModel = new PostDetailModel
            {
                Title = "Funguje to",
            };
            var commentModel = new CommentDetailModel
            {
                Content = "aha"
            };

            userModel = _userRepository.Create(userModel);
            userTwoModel = _userRepository.Create(userTwoModel);
            postModel = _postRepository.Create(postModel, userModel);
            commentModel = _commentRepository.Create(commentModel, userTwoModel, postModel);

            Assert.Empty(postModel.Comments);

            postModel = _postRepository.AddComment(postModel, commentModel);

            Assert.NotEmpty(postModel.Comments);
            Assert.Single(postModel.Comments);

            postModel = _postRepository.RemoveComment(postModel, commentModel);

            Assert.Empty(postModel.Comments);

            //Teardown
            _postRepository.Delete(postModel.Id);

        }
        [Fact]
        public void AddComment_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Dan",
                Email = "drevo@seznam.com",
                Password = "drevobezdreva",

            });

            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Funguje to",
            }, userModel);


            for (int i = 0; i < 5; i++)
            {
                var commModel = new CommentDetailModel
                {
                    Content = i.ToString() + " Komentar k Funguje to",

                };

                commModel = _commentRepository.Create(commModel, userModel, postModel);
                postModel = _postRepository.AddComment(postModel, commModel);
            }

            var commOneModel = new CommentDetailModel
            {
                Content = " Komentar k Funguje to",

            };

            commOneModel = _commentRepository.Create(commOneModel, userModel, postModel);
            postModel = _postRepository.AddComment(postModel, commOneModel);


            Assert.NotNull(postModel);
            Assert.NotEmpty(postModel.Comments);
            Assert.Equal(6, postModel.Comments.Count);

            postModel = _postRepository.RemoveComment(postModel, commOneModel);

            Assert.Equal(5, postModel.Comments.Count);

            //TearDown

            foreach (var comment in postModel.Comments)
            {
                postModel = _postRepository.RemoveComment(postModel, comment);
            }

            _postRepository.Delete(postModel.Id);
            _userRepository.Delete(userModel.Id);

        }



        [Fact]
        public void RemoveComment_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Dan",
                Email = "drevo@seznam.com",
                Password = "drevobezdreva",

            });

            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Funguje to asi",
            }, userModel);



            var commModel = _commentRepository.Create(new CommentDetailModel
            {
                Content = "Komentar k Funguje to asi",

            }, userModel, postModel);

            postModel = _postRepository.AddComment(postModel, commModel);
            postModel = _postRepository.RemoveComment(postModel, commModel);

            Assert.NotNull(postModel);
            Assert.Empty(postModel.Comments);


            _postRepository.Delete(postModel.Id);
            _userRepository.Delete(userModel.Id);


        }

        [Fact]
        public void UpdateCommentContent_PostModel_Test()
        {
            var userModel = _userRepository.Create(new UserDetailModel
            {
                Name = "Zdeno",
                Email = "zdenodrevo@seznam.com",
                Password = "drevobezdreva",

            });

            var postModel = _postRepository.Create(new PostDetailModel
            {
                Title = "Update",
                Content = "aktualizovano",
                CreationTime = DateTime.Now,
            }, userModel);



            var commModel = _commentRepository.Create(new CommentDetailModel
            {
                Content = "tak jde cas",
                CreationTime = DateTime.Now,
            }, userModel, postModel);


            postModel = _postRepository.AddComment(postModel, commModel);

            var returnedCommModel = postModel.Comments.First();

            commModel = _commentRepository.UpdateContent(returnedCommModel, "tak nejde cas");
            postModel = _postRepository.GetById(postModel.Id);

            Assert.NotEmpty(postModel.Comments);
            Assert.Equal("tak nejde cas", postModel.Comments.First().Content);


            _console.WriteLine(postModel.Comments.First().CreationTime.ToString());

            _commentRepository.Delete(commModel.Id);
            _postRepository.Delete(postModel.Id);
            _userRepository.Delete(userModel.Id);
           


        }



    }
}
