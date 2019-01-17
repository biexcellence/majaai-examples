using System;

namespace MajaUWP.ViewModels
{
    public class ViewModelBase : PropertyChangedOnMainThread, IDisposable
    {
        public ViewModelBase()
        {

        }

        public virtual void Dispose()
        {

        }
    }
}