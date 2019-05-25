using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeamChat.DAL.Resources;

namespace TeamChat.DAL
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TeamChatDbContext>
    {
        public TeamChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TeamChatDbContext>();
            optionsBuilder.UseSqlServer(DesignTimeConnectionStrings.LocalDB);
            return new TeamChatDbContext(optionsBuilder.Options);
        }

    }
}
