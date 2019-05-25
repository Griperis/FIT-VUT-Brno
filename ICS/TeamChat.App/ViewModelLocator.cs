using TeamChat.App.ViewModels;
using TeamChat.BL.Factories;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;

namespace TeamChat.App
{
    public class ViewModelLocator
    {
        private readonly IWindowService _windowService = new WindowService();
        private readonly IMediator _mediator;

        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;

        public ViewModelLocator()
        {
            var teamChatDbContextFactory = new TeamChatDbContextFactory();
            _mediator = new Mediator();

            _teamRepository = new TeamRepository(teamChatDbContextFactory);
            _userRepository = new UserRepository(teamChatDbContextFactory);
            _postRepository = new PostRepository(teamChatDbContextFactory);
            _commentRepository = new CommentRepository(teamChatDbContextFactory);
        }

        public MainViewModel MainViewModel => new MainViewModel(_windowService, _teamRepository, _postRepository, _userRepository, _mediator);
        public LoginViewModel LoginViewModel => new LoginViewModel(_windowService, _userRepository, _mediator);
        public RegisterViewModel RegisterViewModel => new RegisterViewModel(_windowService, _userRepository, _mediator);
        public UserViewModel UserViewModel => new UserViewModel(_windowService, _userRepository, _teamRepository, _mediator);
        public TeamViewModel TeamViewModel => new TeamViewModel(_windowService, _teamRepository, _userRepository, _postRepository, _commentRepository, _mediator);
        public NewPostViewModel NewPostViewModel => new NewPostViewModel(_windowService,_teamRepository,_postRepository,_userRepository,_mediator);
        public NewCommentViewModel NewCommentViewModel => new NewCommentViewModel(_postRepository,_userRepository,_commentRepository,_mediator);



    }

}