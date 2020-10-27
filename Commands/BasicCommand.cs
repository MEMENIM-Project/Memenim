using System;
using System.Windows.Input;

namespace Memenim.Commands
{
    public class BasicCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public Func<object, bool> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public BasicCommand(Func<object, bool> canExecute = null, Action<object> execute = null)
        {
            CanExecuteDelegate = canExecute;
            ExecuteDelegate = execute;
        }

        public bool CanExecute(object parameter)
        {
            var canExecute = CanExecuteDelegate;
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteDelegate?.Invoke(parameter);
        }
    }
}
