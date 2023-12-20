using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicPlayer.WPF.Base
{
    public class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Notify([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}