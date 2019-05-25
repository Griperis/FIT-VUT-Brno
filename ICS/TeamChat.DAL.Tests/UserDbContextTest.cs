using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamChat.BL.Interfaces;
using TeamChat.DAL.Entities;
using Xunit;

namespace TeamChat.DAL.Tests
{
    
    public class UserDbContextTest
    {
        private readonly ITeamChatDbContextFactory _userDbContextFactory;

        public UserDbContextTest()
        {
            _userDbContextFactory = new InMemoryDbContextFactory();
        }

        [Fact]
        public void AddOnlyUser()
        {
            //Arrange
            var user = new User()
            {
                Name = "Novy User",
                Email = "NovyEmail@email.com",
                Password = "NoveHeslo"
            };

            //Act
            using (var userdbContext = _userDbContextFactory.CreateTeamChatDbContext())
            {
                userdbContext.Users.Add(user);
                userdbContext.SaveChanges();
            }


            //Assert
            using (var userdbContext = _userDbContextFactory.CreateTeamChatDbContext())
            {
                User retrievedUser = null;

                try
                {
                    retrievedUser = userdbContext.Users.First(t => t == user);
                    Assert.Equal("Novy User", retrievedUser.Name);

                }
                finally
                {
                    //Teardown
                    if (retrievedUser != null)
                    {
                        userdbContext.Users.Remove(retrievedUser);
                    }
                }
            }
        }




    }
}
