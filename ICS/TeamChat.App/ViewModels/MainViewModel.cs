using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.BL.Extensions;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;
using TeamChat.BL.Messages;
using TeamChat.BL.Interfaces;

namespace TeamChat.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;

        private readonly ITeamRepository _teamRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        private readonly IMediator _mediator;


        public ObservableCollection<TeamListModel> TeamMenuItems { get; set; } = new ObservableCollection<TeamListModel>();
        public ObservableCollection<PostDetailModel> Posts { get; set; } = new ObservableCollection<PostDetailModel>();

        public ICommand OpenNewWindowCommand { get; set; }
        public ICommand TeamSelectedCommand { get; set; }
        public ICommand ShowUserInfoCommand { get; set; }
        public ICommand ShowTeamInfoCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand CreateTeamCommand { get; set; }

        public MainViewModel(IWindowService windowService, ITeamRepository teamRepository, IPostRepository postRepository, IUserRepository userRepository, IMediator mediator)
        {
            this._windowService = windowService;
            this._teamRepository = teamRepository;
            this._userRepository = userRepository;
            this._postRepository = postRepository;
            this._mediator = mediator;

            mediator.Register<UserLogedInMessage>(UserLoggedIn);
            mediator.Register<PostsUpdatedMessage>(UpdatePosts);
            mediator.Register<WhoIsCurrentUserMessage>(SendCurrentUserId);
            mediator.Register<CommentsUpdatedMessage>(UpdateComments);
            mediator.Register<TeamUpdatedMessage>(UpdateTeams);
            mediator.Register<TeamDeletedMessage>(UpdatePosts);

            OpenNewWindowCommand = new RelayCommand<string>(OpenNewWindow);
            TeamSelectedCommand = new RelayCommand<TeamListModel>(TeamSelected);
            ShowUserInfoCommand = new RelayCommand<Guid>(ShowUserInfo);
            ShowTeamInfoCommand = new RelayCommand<Guid>(ShowTeamInfo);
            SearchCommand = new RelayCommand(Search);
            CreateTeamCommand = new RelayCommand(CreateTeam);

        }

        private void UpdateTeams(TeamUpdatedMessage teamUpdatedMessage)
        {
            LoadTeamsFromDb();
        }

        
        public TeamListModel SelectedTeam { get; set; }

        public UserDetailModel CurrentUser { get; set; }

        public string SearchText { get; set; }

        private void CreateTeam()
        {
            var newTeam = new TeamDetailModel();
            newTeam.Name = "New Team";
            
            var createdTeam = _teamRepository.Create(newTeam);
            _teamRepository.AddMember(createdTeam, CurrentUser);

            LoadTeamsFromDb();
        }

        private void Search()
        {
            var postWithSearchedText = new HashSet<PostDetailModel>();

            if (!string.IsNullOrEmpty(SearchText))
            {
                SearchInAll(postWithSearchedText);
                Posts.Clear();
                Posts.AddRange(postWithSearchedText);
            }
            else
            {
                LoadPostsByTeamFromDb(SelectedTeam);
            }

            SortPosts();
        }

        private void SearchInAll(HashSet<PostDetailModel> postWithSearchedText)
        {
            foreach (var post in Posts)
            {
                SearchInPost(post, postWithSearchedText);
                foreach (var comment in post.Comments)
                {
                    SearchInComment(comment, postWithSearchedText, post);
                }
            }
        }

        private void SearchInComment(CommentDetailModel comment, HashSet<PostDetailModel> postWithSearchedText, PostDetailModel post)
        {
            if (comment.Content.Contains(SearchText))
            {
                postWithSearchedText.Add(post);
            }
        }

        private void SearchInPost(PostDetailModel post, HashSet<PostDetailModel> postWithSearchedText)
        {
            if (post.Title.Contains(SearchText) || post.Content.Contains(SearchText))
            {
                postWithSearchedText.Add(post);
            }
        }

        private void SortPosts()
        {
  
            List<PostDetailModel> list = new List<PostDetailModel>(Posts);
            list.Sort();
            Posts.Clear();
            Posts.AddRange(list);


        }

        private void UpdatePosts(TeamDeletedMessage teamDeletedMessage)
        {
            if(SelectedTeam != null)
            {
                if(teamDeletedMessage.teamId == SelectedTeam.Id)
                {
                    SelectedTeam = null;
                    
                }
            }
        }

        private void UpdatePosts(PostsUpdatedMessage postsUpdatedMessage)
        {
            if (SelectedTeam == null)
                return;
            
            if (postsUpdatedMessage.post.Title == "")
                return;

            Posts.Add(postsUpdatedMessage.post);
            SortPosts();
            
           
        }

        private void ShowUserInfo(Guid userId)
        {
            OpenNewWindowCommand.Execute("user");
            _mediator.Send(new UserOpenDetailMessage { UserId = userId });
        }

        private void ShowTeamInfo(Guid teamId)
        {
            OpenNewWindowCommand.Execute("team");
            _mediator.Send(new TeamOpenDetailMessage { TeamId = teamId});
        }

        private void OpenNewWindow(string windowName)
        {
            this._windowService.ShowWindow<NewWindow>(windowName);
        }

        private void TeamSelected(TeamListModel team)
        {
            _mediator.Send(new TeamSelectedMessage { TeamId = team.Id });
            SelectedTeam = team;
            LoadPostsByTeamFromDb(team);
            SortPosts();
        }

        private void LoadPostsByTeamFromDb(TeamListModel team)
        {
            Posts.Clear();
            var posts = _postRepository.GetAllByTeamId(team.Id);
            Posts.AddRange(posts);
        }

        private void UserLoggedIn(UserLogedInMessage userLoggedInMessage)
        {
            CurrentUser = _userRepository.GetById(userLoggedInMessage.UserId);

            try
            {
                LoadTeamsFromDb();
            }
            catch
            {
                Console.Out.WriteLine("No teams found!!");
            }
        }

        private void LoadTeamsFromDb()
        {
            if (CurrentUser != null)
            {
                TeamMenuItems.Clear();
                var teams = _teamRepository.GetAllByUserId(CurrentUser.Id);
                TeamMenuItems.AddRange(teams);
            }
        }

        private void SendCurrentUserId(WhoIsCurrentUserMessage whoIsCurrentUserMessage)
        {
            if (CurrentUser != null)
            {
                _mediator.Send(new CurrentUserIdMessage { UserId = CurrentUser.Id });
            }
        }

        private void UpdateComments(CommentsUpdatedMessage commentsUpdatedMessage)
        {
            var newPost = _postRepository.GetById(commentsUpdatedMessage.postId);
            foreach (var post in Posts)
            {
                if(post.Id == newPost.Id)
                {
                    Posts.Remove(post);
                    break;
                }
            }
            Posts.Add(newPost);
            SortPosts();

        }
    }
}