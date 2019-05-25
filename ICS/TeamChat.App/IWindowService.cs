namespace TeamChat.App
{
    public interface IWindowService
    {
        void ShowWindow<T>(string windowName) where T : class, IViewBase, new();
    }
}