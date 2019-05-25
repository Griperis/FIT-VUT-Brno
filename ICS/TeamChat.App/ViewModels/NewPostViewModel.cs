using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Messages;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;

namespace TeamChat.App.ViewModels
{
    public class NewPostViewModel : ViewModelBase
    {
        private readonly IPostRepository _postRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public ICommand AddNewPostCommand { get; set; }

        public NewPostViewModel(IWindowService windowService, ITeamRepository teamRepository, IPostRepository postRepository, IUserRepository userRepository,IMediator mediator)
        {
            this._mediator = mediator;
            this._postRepository = postRepository;
            this._teamRepository = teamRepository;
            this._userRepository = userRepository;

            AddNewPostCommand = new RelayCommand(AddNewPost);

            mediator.Register<TeamSelectedMessage>(TeamSelected);
            mediator.Register<TeamDeletedMessage>(TeamDeleted);
            mediator.Register<UserLogedInMessage>(UserLoggedIn);
        }



        public string Title { get; set; }
        public string Content { get; set; }

        public TeamDetailModel SelectedTeam { get; set; }
        public UserDetailModel CurrentUser { get; set; }


        private void UserLoggedIn(UserLogedInMessage userLoggedInMessage)
        {
            CurrentUser = _userRepository.GetById(userLoggedInMessage.UserId);
        }

        private void TeamSelected(TeamSelectedMessage teamSelectedMessage)
        {
            SelectedTeam = _teamRepository.GetById(teamSelectedMessage.TeamId);
        }

        private void TeamDeleted(TeamDeletedMessage teamDeletedMessage)
        {
            if(SelectedTeam != null)
            {
                if (teamDeletedMessage.teamId == SelectedTeam.Id)
                    SelectedTeam = null;
            }
            
        }
        
        private void AddNewPost()
        {
            try
            {
                var team = _teamRepository.GetById(SelectedTeam.Id);
            }
            catch
            {
                Title = "";
                Content = "";
                return;
            }
            
            var newPost = new PostDetailModel();
            newPost.Title = Title;
            newPost.Content = Content;
            var returnedPost = _postRepository.Create(newPost, CurrentUser);

            _teamRepository.AddPost(SelectedTeam,returnedPost);

            Title = "";
            Content = "";

            _mediator.Send(new PostsUpdatedMessage{post = returnedPost});

        }

    }
}
