using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Memenim.Commands
{
    public class AsyncBasicCommand : ICommand
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
        public Func<object, Task> ExecuteDelegate { get; set; }

        public AsyncBasicCommand(Func<object, bool> canExecute = null,
            Func<object, Task> execute = null)
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
