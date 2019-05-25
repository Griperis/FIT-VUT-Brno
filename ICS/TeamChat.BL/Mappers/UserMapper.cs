using System.Linq;
using TeamChat.BL.Model;
using TeamChat.DAL.Entities;

namespace TeamChat.BL.Mappers
{
    internal static class UserMapper
    {
        public static UserDetailModel MapToDetailModel(User user)
        {
            var detailModel =  new UserDetailModel()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                LastLoginTime = user.LastLoginTime
            };

            foreach (var activity in user.Activities)
            {
                detailModel.Activities.Add(ActivityMapper.MapToDetailModel(activity));
            }

            foreach (var team in user.Teams)
            {
                detailModel.Teams.Add(TeamMapper.MapToListModel(team.Team));
            }

            return detailModel;

        }
        public static UserListModel MapToListModel(User user)
        {
            return new UserListModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                LastLoginTime = user.LastLoginTime
            };
        }

        public static User MapToEntity(UserDetailModel detailModel)
        {
            var user =  new User
            {
                Id = detailModel.Id,
                Name = detailModel.Name,
                Email = detailModel.Email,
                Password = detailModel.Password,
                LastLoginTime = detailModel.LastLoginTime

            };
            foreach (var activity in detailModel.Activities)
            {
                user.Activities.Add(ActivityMapper.MapToEntity(activity));
            }

            foreach (var team in detailModel.Teams)
            {
                user.Teams.Add(TeamUserMapper.MapToEntity(team, MapToListModel(user)));
            }

            return user;
        }

        public static User MapListModelToEntity(UserListModel listModel)
        {
            return new User
            {
                Id = listModel.Id,
                Name = listModel.Name,
                Email = listModel.Email,
                Password = listModel.Password,
                LastLoginTime = listModel.LastLoginTime
            };
        }
        public static User MapWithPasswordHashToEntity(UserDetailModel detailModel)
        {
            var passwordHandler = new PasswordHandler();
            var user =  new User
            {
                Id = detailModel.Id,
                Name = detailModel.Name,
                Email = detailModel.Email,
                Password = passwordHandler.HashPassword(detailModel.Password),
            };
            foreach (var activity in detailModel.Activities)
            {
                user.Activities.Add(ActivityMapper.MapToEntity(activity));
            }
            foreach (var team in detailModel.Teams)
            {
                user.Teams.Add(TeamUserMapper.MapToEntity(team, MapToListModel(user)));
            }

            return user;
        }

        public static UserDetailModel ListToDetailModel(UserListModel userListModel)
        {
            return new UserDetailModel
            {
                Id = userListModel.Id,
                Name = userListModel.Name,
                Email = userListModel.Email,
                Password = userListModel.Password,
                LastLoginTime = userListModel.LastLoginTime

            };
        }

        public static UserListModel DetailToListModel(UserDetailModel userDetailModel)
        {
            return new UserListModel
            {
                Id = userDetailModel.Id,
                Email = userDetailModel.Email,
                Name = userDetailModel.Name,
                Password = userDetailModel.Password,
                LastLoginTime = userDetailModel.LastLoginTime
            };
        }
    }
}
