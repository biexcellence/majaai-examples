using System;
using System.Windows.Input;

namespace MajaUWP.Utilities
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> _action;
        private Func<bool> _canExecute;
        public Command(Action<object> action, Func<bool> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public Command(Action action, Func<bool> canExecute = null)
            : this(o => action(), canExecute)
        {
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute.Invoke();
            return true;
        }

        public void Execute(object parameter)
        {
            _action.Invoke(parameter);
        }
    }
}
