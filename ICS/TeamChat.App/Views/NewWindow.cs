using TeamChat.App.Views;

namespace TeamChat.App
{
    public class NewWindow : IViewBase
    {

        public void Show(string windowName)
        {

            switch (windowName)
            {
                case "user":
                    new UserWindow().Show();
                    return;
                case "team":
                    new TeamView().Show();
                    return;
                case "main":
                    new MainView().Show();
                    return;
                case "login":
                    new LoginView().Show();
                    return;
                case "register":
                    new RegisterView().Show();
                    return;
            }

        }
    }
}