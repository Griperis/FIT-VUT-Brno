using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamChat.BL.Interfaces;
using TeamChat.DAL.Entities;
using Xunit;

namespace TeamChat.DAL.Tests
{
    [TestCaseOrderer("FullNameOfOrderStrategyHere", "OrderStrategyAssemblyName")]
    public class TeamDbContextTest
    {
        private readonly ITeamChatDbContextFactory _teamDbContextFactory;
        private readonly ITeamChatDbContextFactory _userTeamDbContextFactory;

        public TeamDbContextTest()
        {
            _teamDbContextFactory = new InMemoryDbContextFactory();
            _userTeamDbContextFactory = new InMemoryDbContextFactory();
        }


        [Fact,TestPriority(0)]
        public void CreateTeamTest()
        {
            //Arrange
            var team = new Team()
            {
                Name = "Test Team"
            };

            //Act
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                teamdbContext.Teams.Add(team);
                teamdbContext.SaveChanges();
            }

            //Assert
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                Team retrievedTeam = null;
                try
                {
                    retrievedTeam = teamdbContext.Teams.First(t => t.Id == team.Id);
                    Assert.NotNull(retrievedTeam);
                    Assert.Equal("Test Team", retrievedTeam.Name);
                }
                finally
                {
                    //Teardown
                    if (retrievedTeam != null)
                    {
                        teamdbContext.Teams.Remove(retrievedTeam);
                    }
                }
            }
        }



        [Fact,TestPriority(1)]
        public void AddOnlyTeam()
        {
            //Arrange
            var newTeam = new Team()
            {
                Name = "Novy team",
                Members = { },
                Posts = { }
            };

            //Act
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                teamdbContext.Teams.Add(newTeam);
                teamdbContext.SaveChanges();
            }

            //Assert
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                Team retrievedTeam = null;

                try
                {
                    retrievedTeam = teamdbContext.Teams.First();
                    Assert.NotNull(retrievedTeam);
                }
                finally
                {
                    //Teardown
                    if (retrievedTeam != null)
                    {
                        teamdbContext.Teams.Remove(retrievedTeam);
                    }
                }
            }
        }


        [Fact,TestPriority(2)]
        public void AddUsertoTeam()
        {
            //Arrange
            var team1 = new Team()
            {
                Name = "Novy Team1"
            };

            var user1 = new User()
            {
                Name = "Novy User",
                Email = "NovyEmail@email.com",
                Password = "NoveHeslo",

            };
            var user2 = new User()
            {
                Name = "Novy User2",
                Email = "NovyEmail2@email.com",
                Password = "NoveHeslo2"
            };

            var member1 = new TeamUser()
            {
                Team = team1,
                User = user1
            };

            var member2 = new TeamUser()
            {
                Team = team1,
                User = user2
            };

            var team1post = new Post()
            {
                Author = user1,
                Content = "Test Content",
                CreationTime = DateTime.Now,
                Title = "Test Title"
            };

            //Act
            using (var uteamdbContext = _userTeamDbContextFactory.CreateTeamChatDbContext())
            {
                uteamdbContext.Users.Add(user1);
                uteamdbContext.Teams.Add(team1);
                uteamdbContext.TeamUsers.Add(member1);
                uteamdbContext.TeamUsers.Add(member2);
                uteamdbContext.Posts.Add(team1post);
                uteamdbContext.SaveChanges();
            }


            //Assert
            using (var uteamdbContext = _userTeamDbContextFactory.CreateTeamChatDbContext())
            {
                Team retrievedUserTeam = null;
                TeamUser retrievedTeamMember1 = null;
                TeamUser retrievedTeamMember2 = null;
                Post retrievedPost = null;

                try
                {
                    retrievedUserTeam = uteamdbContext.Teams.First();
                    Assert.Equal("Novy Team1", retrievedUserTeam.Name);
                    retrievedTeamMember1 = uteamdbContext.TeamUsers.First(t => t.User == user1);
                    Assert.Equal("Novy Team1", retrievedTeamMember1.Team.Name);
                    retrievedTeamMember2 = uteamdbContext.TeamUsers.First(t => t.User == user2);
                    Assert.Equal("Novy Team1", retrievedTeamMember2.Team.Name);
                    retrievedPost = uteamdbContext.Posts.First(t => t.Author == user1);
                    Assert.Equal("Test Content", retrievedPost.Content);
                    Assert.Equal("Test Title", retrievedPost.Title);
                }
                finally
                {
                    //Teardown
                    if (retrievedUserTeam != null)
                    {
                        uteamdbContext.Teams.Remove(retrievedUserTeam);
                    }

                    if (retrievedTeamMember1 != null)
                    {
                        uteamdbContext.TeamUsers.Remove(retrievedTeamMember1);
                    }

                    if (retrievedTeamMember2 != null)
                    {
                        uteamdbContext.TeamUsers.Remove(retrievedTeamMember2);
                    }

                    if (retrievedPost != null)
                    {
                        uteamdbContext.Posts.Remove(retrievedPost);
                    }
                }
            }
        }


        [Fact,TestPriority(3)]
        public void AddOnlyTeamMember()
        {
            //Arrange
            var teamMember = new TeamUser()
            {
                Team = new Team(),
                User = new User(),
            };

            //Act
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                teamdbContext.TeamUsers.Add(teamMember);
                teamdbContext.SaveChanges();
            }


            //Assert
            using (var teamdbContext = _teamDbContextFactory.CreateTeamChatDbContext())
            {
                TeamUser retrievedTeamMember = null;

                try
                {
                    retrievedTeamMember = teamdbContext.TeamUsers.First();
                    Assert.NotNull(retrievedTeamMember);

                }
                finally
                {
                    //Teardown
                    if (retrievedTeamMember != null)
                    {
                        teamdbContext.TeamUsers.Remove(retrievedTeamMember);
                    }
                }
            }
        }
    }
}
