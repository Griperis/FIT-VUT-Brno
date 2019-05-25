using TeamChat.DAL;

namespace TeamChat.BL.Interfaces
{
    public interface ITeamChatDbContextFactory
    {
        TeamChatDbContext CreateTeamChatDbContext();
    }
}
