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



        private Func<object, bool> CanExecuteDelegate { get; set; }
        private Action<object> ExecuteDelegate { get; set; }



        public BasicCommand(
            Action<object> executeDelegate = null,
            Func<object, bool> canExecuteDelegate = null)
        {
            ExecuteDelegate = executeDelegate;
            CanExecuteDelegate = canExecuteDelegate;
        }



        public bool CanExecute(
            object parameter)
        {
            return CanExecuteDelegate == null
                   || CanExecuteDelegate(parameter);
        }

        public void Execute(
            object parameter)
        {
            ExecuteDelegate?.Invoke(parameter);
        }
    }
}
