namespace TeamChat.App
{
    public class WindowService : IWindowService
    {
        public void ShowWindow<T>(string windowName) where T : class, IViewBase, new()
        {
            var window = new T();
            window.Show(windowName);
        }
    }
}