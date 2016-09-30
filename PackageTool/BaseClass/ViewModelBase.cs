using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PackageTool.Annotations;

namespace PackageTool.BaseClass
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(property, value))
            {
                return;
            }
            property = value;
            OnPropertyChanged(propertyName);
        }

        public void Close()
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                }
            }
        }

        public System.Windows.Window GetCurrentWindow()
        {
            System.Windows.Window currentWindow = null;
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    currentWindow = window;
                }
            }
            return currentWindow;
        }
    }
}
