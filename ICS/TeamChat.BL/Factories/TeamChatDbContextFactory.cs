using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Resources;
using TeamChat.DAL;

namespace TeamChat.BL.Factories
{
    public class TeamChatDbContextFactory : ITeamChatDbContextFactory
    {
        public TeamChatDbContext CreateTeamChatDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TeamChatDbContext>();
            optionsBuilder.UseSqlServer(ConnectionStrings.LocalDB);
        return new TeamChatDbContext(optionsBuilder.Options);
        }
    }
}
