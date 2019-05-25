using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TeamChat.App.ViewModels.Base;
using TeamChat.BL;
using TeamChat.BL.Interfaces;
using TeamChat.BL.Model;
using TeamChat.BL.Repositories;
using TeamChat.BL.Services;

namespace TeamChat.App.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IWindowService _windowService;
        private readonly IUserRepository _userRepository;
        private PasswordHandler _passwordHandler;
        private readonly IMediator _mediator;

        public ICommand CreateNewUserCommand { get; set; }
        public ICommand OpenNewWindowCommand { get; set; }

        public RelayCommand<Window> CloseWindowCommand { get; private set; }

        public RegisterViewModel(IWindowService windowService, IUserRepository userRepository, IMediator mediator)
        {
            this._windowService = windowService;
            this._userRepository = userRepository;
            this._mediator = mediator;

            OpenNewWindowCommand = new RelayCommand<string>(OpenNewWindow);
            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);
            CreateNewUserCommand = new RelayCommand(CreateUser);

            this._passwordHandler = new PasswordHandler();
        }

        public UserDetailModel User = new UserDetailModel();
        public string EnteredPassword { get; set; }
        public string EnteredEmail { get; set; }
        public string EnteredName { get; set; }
        public string InfoLabel { get; set; }


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

        private async void CreateUser()
        {
            if (EnteredEmail == null || !CheckEmail(EnteredEmail)) { InfoLabel = "Invalid email"; return; }
            if (EnteredName == null || EnteredName.Length == 0) { InfoLabel = "Invalid name"; return; }
            if (EnteredPassword == null || EnteredPassword.Length == 0) { InfoLabel = "Invalid password"; return; }

            
            User.Name = EnteredName;
            User.Email = EnteredEmail;
            User.Password = EnteredPassword;

            InfoLabel = "Checking users";

            if ( await UserExists(EnteredEmail))
            {
                InfoLabel = "User already exists!";
            }
            else
            {
                InfoLabel = "Creating new user!";
                await CreateNewUserAsync(User);

                InfoLabel = "User Created!";
                this.CloseWindowCommand.Execute(GetWindow("RegisterWindow"));
            }
        }

        private Task CreateNewUserAsync(UserDetailModel user)
        {
            return Task.Run(() =>
            {
                _userRepository.Create(user);
            });
        }

        private Task<bool> UserExists(string email)
        {

            return Task.Run(() =>
            {
                try
                {
                    _userRepository.GetByEmail(email);
                }
                catch
                {
                    return false;
                }

                return true;
            });
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
