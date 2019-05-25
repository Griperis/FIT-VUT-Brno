using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Controls;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.BL.Extensions;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;
using TeamChat.BL.Messages;
using TeamChat.App.Views;
using System.Threading.Tasks;
using System.Windows;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using TeamChat.BL.Interfaces;

namespace TeamChat.App.ViewModels
{
    public class TeamViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;

        private readonly IMediator _mediator;

        public TeamDetailModel Team { get; set; }
        public UserListModel SelectedMember { get; set; }
        public UserListModel SelectedUser { get; set; }

        public string PostCount { get; set; }
        public string MembersCount { get; set; }
        public string InfoLabel { get; set; }

        public ObservableCollection<UserListModel> Members { get; set; } = new ObservableCollection<UserListModel>();
        public ObservableCollection<UserListModel> AllUsers { get; set; } = new ObservableCollection<UserListModel>();

        public ICommand MemberSelectedCommand { get; set; }
        public ICommand UserSelectedCommand { get; set; }

        public ICommand AddTeamMemberCommand { get; set; }
        public ICommand RemoveTeamMemberCommand { get; set; }

        public ICommand RenameTeamCommand { get; set; }
        public ICommand DeleteTeamCommand { get; set; }

        public RelayCommand<Window> CloseWindowCommand { get; private set; }


        public TeamViewModel(IWindowService windowService, ITeamRepository teamRepository, IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository, IMediator mediator)
        {
            this._windowService = windowService;
            this._teamRepository = teamRepository;
            this._userRepository = userRepository;
            this._postRepository = postRepository;
            this._commentRepository = commentRepository;
            this._mediator = mediator;

            mediator.Register<TeamOpenDetailMessage>(TeamOpenDetail);

            MemberSelectedCommand = new RelayCommand<UserListModel>(MemberSelected);
            UserSelectedCommand = new RelayCommand<UserListModel>(UserSelected);

            AddTeamMemberCommand = new RelayCommand(AddTeamMember);
            RemoveTeamMemberCommand = new RelayCommand(RemoveTeamMember);

            RenameTeamCommand = new RelayCommand(RenameTeam);
            DeleteTeamCommand = new RelayCommand(DeleteTeam);

            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);

        }

        private void UserSelected(UserListModel user)
        {
            SelectedUser = user;
        }

        private void DeletePosts(IEnumerable<PostDetailModel> posts)
        {
            foreach (var post in posts)
            {
                DeleteComments(post);
                Team = _teamRepository.RemovePost(Team, post);
                _postRepository.Delete(post);
            }
        }

        private void DeleteComments(PostDetailModel post)
        {
            foreach(var comment in post.Comments)
            {
                _postRepository.RemoveComment(post, comment);
                _commentRepository.Delete(comment);
            }
        }
        private void DeleteTeam()
        {
            var teamId = Team.Id;
            DeletePosts(Team.Posts);
            _teamRepository.Delete(Team);
            _mediator.Send(new TeamUpdatedMessage());
            _mediator.Send(new TeamDeletedMessage { teamId = teamId});
            this.CloseWindowCommand.Execute(GetWindow("TeamWindow"));
        }

        private void RenameTeam()
        {
            _teamRepository.UpdateName(Team.Id, Team.Name);
            _mediator.Send(new TeamUpdatedMessage());
        }

        private void RemoveTeamMember()
        {
            if (SelectedMember != null)
            {
                Team = _teamRepository.RemoveMember(Team, _userRepository.GetById(SelectedMember.Id));
                SelectedMember = null;
                LoadTeam(Team.Id);
                InfoLabel = "User removed from the team!";
            }
            else
            {
                InfoLabel = "No user selected!";
            }
        }

        private void AddTeamMember()
        {
            try
            {
                _teamRepository.AddMember(Team, _userRepository.GetById(SelectedUser.Id));
                LoadTeam(Team.Id);
                InfoLabel = "New member added!";
            }
            catch
            {
                InfoLabel = "Member is already in team!";
            }
        }

        private void MemberSelected(UserListModel user)
        {
            SelectedMember = user;
        }

        private void LoadTeam(Guid id)
        {
            Team = _teamRepository.GetById(id);
            LoadTeamMembers();
            LoadAllUsers();
            LoadTeamInfo();
            _mediator.Send(new TeamUpdatedMessage());
        }

        private void TeamOpenDetail(TeamOpenDetailMessage teamOpenDetailMessage)
        {
            try
            {
                LoadTeam(teamOpenDetailMessage.TeamId);
            }
            catch
            {
                Console.Out.WriteLine("Team does not exist!");
            }
        }

        private void LoadAllUsers()
        {
            AllUsers.Clear();
            var users = _userRepository.GetAll();
            AllUsers.AddRange(users);
        }

        private void LoadTeamInfo()
        {
            PostCount = (Team.Posts.Count).ToString();
            MembersCount = (Team.Members.Count).ToString();
        }

        private void LoadTeamMembers()
        {
            Members.Clear();
            var members = Team.Members;
            Members.AddRange(members);
        }

        public Window GetWindow(string name)
        {
            Window windowObject = null;
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Name == name)
                {
                    windowObject = window;
                }
            }
            return windowObject;
        }

        private void CloseWindow(Window window)
        {
            window?.Close();
        }

    }

}