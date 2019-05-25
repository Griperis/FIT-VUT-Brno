using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
    public class NewCommentViewModel : ViewModelBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IMediator _mediator;

        public ICommand AddNewCommentCommand { get; set; }

        public NewCommentViewModel(IPostRepository postRepository, IUserRepository userRepository, ICommentRepository commentRepository, IMediator mediator)
        {
            this._mediator = mediator;
            this._postRepository = postRepository;
            this._userRepository = userRepository;
            this._commentRepository = commentRepository;

            AddNewCommentCommand = new RelayCommand<Guid>(AddNewComment);

            mediator.Register<CurrentUserIdMessage>(SetCurrentUser);

        }

        public string Content { get; set; }
        public UserDetailModel CurrentUser { get; set; }


        private void SetCurrentUser(CurrentUserIdMessage userLoggedInMessage)
        {
            CurrentUser = _userRepository.GetById(userLoggedInMessage.UserId);
        }

        private async void AddNewComment(Guid postId)
        {   
            if(Content == null || Content.Length == 0)
            {
                return;
            }

            if (CurrentUser == null)
            {
                _mediator.Send(new WhoIsCurrentUserMessage());
            }

            await AddCommentAsync(postId);

            _mediator.Send(new CommentsUpdatedMessage{postId = postId});
            
            Content = "";

        }

        private Task AddCommentAsync(Guid postId)
        {
            return Task.Run(() =>
            {
                var post = _postRepository.GetById(postId);

                var comment = new CommentDetailModel();
                comment.Content = Content;

                var returnedComment = _commentRepository.Create(comment, CurrentUser, post);

                _postRepository.AddComment(post, returnedComment);

            });
        }
    }

}
