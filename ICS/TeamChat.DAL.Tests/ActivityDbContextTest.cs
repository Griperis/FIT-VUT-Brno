using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamChat.BL.Interfaces;
using TeamChat.DAL.Entities;
using Xunit;

namespace TeamChat.DAL.Tests
{
    public class ActivityDbContextTest
    {
        private readonly ITeamChatDbContextFactory _activityDbContextFactory;

        public ActivityDbContextTest()
        {
            _activityDbContextFactory = new InMemoryDbContextFactory();
        }

        [Fact]
        public void CreateActivityTest()
        {
            {
                //Arrange
                var user = new User()
                {
                    Name = "Dano Drevo"
                };
                var user2 = new User()
                {
                    Name = "Dano bez dreva",
                    Email = "nemam@seznam.cz"
                };
                var post = new Post()
                {
                    Author = user,
                    Content = "Ahoj dano",
                    CreationTime = DateTime.Now,
                    Title = "TITUL"

                };
                var comment = new Comment()
                {
                    Author = user,
                    BelongsTo = post,
                    Content = "velky spatny"
                };
                //Act
                using (var actdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
                {
                    actdbContext.Users.Add(user);
                    actdbContext.Users.Add(user2);
                    actdbContext.Posts.Add(post);
                    actdbContext.Activities.Add(comment);
                    actdbContext.SaveChanges();
                }

                //Assert
                using (var actdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
                {
                    Activity retrievedPost = null;
                    Activity retrievedComment = null;
                    List<Activity> activities = new List<Activity>();

                    try
                    {
                        retrievedPost = actdbContext.Posts.First(t => t.Author == user);
                        Assert.Equal("Ahoj dano", retrievedPost.Content);
                        retrievedComment = actdbContext.Comments.First(t => t.BelongsTo == retrievedPost);
                        Assert.Equal("velky spatny", retrievedComment.Content);
                        activities = actdbContext.Activities.Where(t => t.Author == user).ToList();
                        Assert.NotNull(activities);
                        Assert.Equal("Ahoj dano", activities[1].Content);
                        Assert.Equal("velky spatny", activities[0].Content);

                    }
                    finally
                    {
                        //Teardown
                        if (retrievedPost != null)
                        {
                            actdbContext.Activities.Remove(retrievedPost);
                        }

                        if (retrievedComment != null)
                        {
                            actdbContext.Activities.Remove(retrievedComment);
                        }
                    }
                }
            }
        }

        [Fact]
        public void AddOnlyComment()
        {
            //Arrange
            var comment = new Comment()
            {
                BelongsTo = new Post()
            };

            //Act
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                comdbContext.Comments.Add(comment);
                comdbContext.SaveChanges();
            }

            //Assert
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                Comment retrievedComment = null;

                try
                {
                    retrievedComment = comdbContext.Comments.First();
                    Assert.NotNull(retrievedComment);
                }
                finally
                {
                    //Teardown
                    if (retrievedComment != null)
                    {
                        comdbContext.Comments.Remove(retrievedComment);
                    }
                }
            }
        }

        [Fact]
        public void AddOnlyPost()
        {
            //Arrange
            var post = new Post()
            {
                Title = "Novy title",
                Comments = { }
            };

            //Act
            using (var postdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                postdbContext.Posts.Add(post);
                postdbContext.SaveChanges();
            }

            //Assert
            using (var postdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                Post retrievedPost = null;

                try
                {
                    retrievedPost = postdbContext.Posts.First();
                    Assert.NotNull(retrievedPost);
                }
                finally
                {
                    //Teardown
                    if (retrievedPost != null)
                    {
                        postdbContext.Posts.Remove(retrievedPost);
                    }
                }
            }
        }




        [Fact]
        public void ChangeContentOfComment()
        {
            //Arrange
            var user = new User()
            {
                Name = "Novy User",
                Email = "NovyEmail@email.com",
                Password = "NoveHeslo"
            };

            var post = new Post()
            {
                Author = user,
                Content = "Test Content",
                CreationTime = DateTime.Now,
                Title = "Test Title"
            };

            var comment = new Comment()
            {
                BelongsTo = post,
                Author = user,
                Content = post.Content,
                CreationTime = post.CreationTime,
            };

            var newCommentContent = new Comment()
            {
                BelongsTo = post,
                Author = user,
                Content = "New Comment Content",
                CreationTime = DateTime.Now,
            };

            //Act
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                comdbContext.Users.Add(user);
                comdbContext.Posts.Add(post);
                comdbContext.Comments.Add(comment);
                comdbContext.Comments.Remove(comment);
                comdbContext.Comments.Add(newCommentContent);
                comdbContext.SaveChanges();
            }


            //Assert
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                Comment retrievedComment = null;

                try
                {
                    retrievedComment = comdbContext.Comments.First(t => t.Author == user);
                    Assert.Equal("New Comment Content", retrievedComment.Content);

                }
                finally
                {
                    //Teardown
                    if (retrievedComment != null)
                    {
                        comdbContext.Comments.Remove(retrievedComment);
                    }
                }
            }
        }

        [Fact]
        public void CreateComment()
        {
            //Arrange
            var user = new User()
            {
                Name = "Novy User",
                Email = "NovyEmail@email.com",
                Password = "NoveHeslo"
            };

            var post = new Post()
            {
                Author = user,
                Content = "Test Content",
                CreationTime = DateTime.Now,
                Title = "Test Title"
            };

            var comment = new Comment()
            {
                BelongsTo = post,
                Author = user,
                Content = post.Content,
                CreationTime = post.CreationTime,

            };

            //Act
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                comdbContext.Users.Add(user);
                comdbContext.Posts.Add(post);
                comdbContext.Comments.Add(comment);
                comdbContext.SaveChanges();
            }


            //Assert
            using (var comdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                Comment retrievedComment = null;

                try
                {
                    retrievedComment = comdbContext.Comments.First(t => t.Author == user);
                    Assert.Equal("Test Content", retrievedComment.Content);

                }
                finally
                {
                    //Teardown
                    if (retrievedComment != null)
                    {
                        comdbContext.Comments.Remove(retrievedComment);
                    }
                }
            }
        }

        [Fact]
        public void CreatePostTest()
        {
            //Arrange
            var user = new User()
            {
                Name = "Novy User"
            };
            var post = new Post()
            {
                Author = user,
                Content = "Test Content",
                CreationTime = DateTime.Now,
                Title = "Test Title"
            };
            var comment = new Comment()
            {
                Author = user,
                BelongsTo = post,
                Content = "novy content"
            };

            //Act
            using (var postdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                postdbContext.Users.Add(user);
                postdbContext.Posts.Add(post);
                postdbContext.Activities.Add(comment);
                postdbContext.SaveChanges();
            }


            //Assert
            using (var postdbContext = _activityDbContextFactory.CreateTeamChatDbContext())
            {
                Activity retrievedPost = null;
                Activity retrievedComment = null;
                List<Activity> activities = new List<Activity>();

                try
                {
                    retrievedPost = postdbContext.Posts.First(t => t.Author == user);
                    Assert.Equal("Test Content", retrievedPost.Content);

                    retrievedComment = postdbContext.Comments.First(t => t.BelongsTo == retrievedPost);
                    Assert.Equal("novy content", retrievedComment.Content);

                    activities = postdbContext.Activities.Where(t => t.Author == user).ToList();
                    Assert.NotNull(activities);
                    Assert.Equal("Test Content", activities[1].Content);
                    Assert.Equal("novy content", activities[0].Content);

                }
                finally
                {
                    //Teardown
                    if (retrievedPost != null)
                    {
                        postdbContext.Activities.Remove(retrievedPost);
                    }

                    if (retrievedComment != null)
                    {
                        postdbContext.Activities.Remove(retrievedComment);
                    }
                }
            }
        }
    }
}
