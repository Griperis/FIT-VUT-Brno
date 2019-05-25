using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.BL.Extensions;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Messages;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;

namespace TeamChat.App.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;

        private readonly IMediator _mediator;

        public ObservableCollection<TeamListModel> TeamListItems { get; set; } = new ObservableCollection<TeamListModel>();
        public ObservableCollection<CommentDetailModel> CommentActivityListItems { get; set; } = new ObservableCollection<CommentDetailModel>();
        public ObservableCollection<PostListModel> PostActivityListItems { get; set; } = new ObservableCollection<PostListModel>();
        public ICommand ShowTeamInfoCommand { get; set; }
        public ICommand OpenNewWindowCommand { get; set; }


        public UserViewModel(IWindowService windowService, IUserRepository userRepository, ITeamRepository teamRepository, IMediator mediator)
        {
            this._windowService = windowService;
            this._userRepository = userRepository;
            this._teamRepository = teamRepository;
            this._mediator = mediator;

            _mediator.Register<UserOpenDetailMessage>(UserOpenDetail);
            ShowTeamInfoCommand = new RelayCommand<Guid>(ShowTeamInfo);
            OpenNewWindowCommand = new RelayCommand<string>(OpenNewWindow);

        }

        public UserDetailModel User { get; set; }

        private void ShowTeamInfo(Guid teamId)
        {
            OpenNewWindowCommand.Execute("team");
            _mediator.Send(new TeamOpenDetailMessage { TeamId = teamId });
        }
        private void OpenNewWindow(string windowName)
        {
            this._windowService.ShowWindow<NewWindow>(windowName);
        }
        private void UserOpenDetail(UserOpenDetailMessage userOpenDetailMessage)
        {
            try
            {
                if (User == null)
                {
                    User = _userRepository.GetById(userOpenDetailMessage.UserId);
                }
            }
            catch
            {
                Console.Out.WriteLine("User does not exist!");
            }

            try
            {
                SetupActivitiesProperties();
            }
            catch
            {
                Console.Out.WriteLine("Loading activities error");
            }

            try
            {
                TeamListItems.Clear();
                var teams = _teamRepository.GetAllByUserId(User.Id);
                TeamListItems.AddRange(teams);
            }
            catch
            {
                Console.Out.WriteLine("Loading teams error");
            }

        }

        private void SetupActivitiesProperties()
        {
            foreach (var activity in _userRepository.GetActivitiesById(User.Id))
            {
                if (activity is CommentDetailModel comment)
                {
                    CommentActivityListItems.Add(comment);
                }
                else if (activity is PostListModel post)
                {
                    PostActivityListItems.Add(post);
                }
            }
        }
    }
}