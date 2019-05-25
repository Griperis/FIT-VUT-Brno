using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TeamChat.App.Annotations;

namespace TeamChat.App.ViewModels.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IViewModel
    {

        public virtual void Load()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}