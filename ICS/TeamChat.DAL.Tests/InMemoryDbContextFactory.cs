using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeamChat.BL.Interfaces;
using TeamChat.DAL.Interfaces;
using Xunit;

namespace TeamChat.DAL.Tests
{
    public class InMemoryDbContextFactory : ITeamChatDbContextFactory
    {

        public TeamChatDbContext CreateTeamChatDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TeamChatDbContext>();
            optionsBuilder.UseInMemoryDatabase("TeamChatDb");
            return new TeamChatDbContext(optionsBuilder.Options);
        }

    }
}
