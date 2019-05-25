using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.App.Views;
using TeamChat.BL;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Messages;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;

namespace TeamChat.App.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserRepository _userRepository;
        private readonly PasswordHandler _passwordHandler;
        private readonly IMediator _mediator;

        public ICommand ValidateUserCommand { get; set; }
        public ICommand OpenNewWindowCommand { get; set; }

        public RelayCommand<Window> CloseWindowCommand { get; private set; }
        

        public LoginViewModel(IWindowService windowService, IUserRepository userRepository, IMediator mediator)
        {
            this._windowService = windowService;
            this._userRepository = userRepository;
            this._mediator = mediator;

            OpenNewWindowCommand = new RelayCommand<string>(OpenNewWindow);
            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);

            this._passwordHandler = new PasswordHandler();

            ValidateUserCommand = new RelayCommand(ValidateUser);
        }

        public UserDetailModel User { get; set; }
        public string EnteredPassword { get; set; }
        public string EnteredEmail { get; set; }
        public string InfoLabel { get; set; }



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

        private async void ValidateUser()
        {
            InfoLabel = "Loading User!";
            User = await LoadUserByEmailAsync(EnteredEmail);

           
            if (User != null)
            {
                CheckPassword();
            }
            else
            {
                InfoLabel = "User does not exist!";
            }

        }

        private bool CheckEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }

        }

        private void CheckPassword()
        {
            if (EnteredPassword != null && _passwordHandler.IsCorrectPassword(EnteredPassword, User.Password))
            {
                _userRepository.SetUserLoginTime(User);
                this.OpenNewWindowCommand.Execute("main");
                _mediator.Send(new UserLogedInMessage {UserId = User.Id});
                this.CloseWindowCommand.Execute(GetWindow("LoginWindow"));
            }
            else
            {
                InfoLabel = "Wrong password!";
            }
        }

        private Task<UserDetailModel> LoadUserByEmailAsync(string email)
        {
            return Task.Run(() =>
            {              
                UserDetailModel user;
                try
                {
                     user = _userRepository.GetByEmail(email);
                }
                catch
                {
                    user = null;
                }
                return user;
            });
        }


        private void OpenNewWindow(string windowName)
        {
            this._windowService.ShowWindow<NewWindow>(windowName);
        }

        private void CloseWindow(Window window)
        {
            window?.Close();
        }
    }
}