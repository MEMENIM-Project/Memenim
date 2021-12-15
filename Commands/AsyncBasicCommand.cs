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



        private Func<object, bool> CanExecuteDelegate { get; set; }
        private Func<object, Task> ExecuteDelegate { get; set; }



        public AsyncBasicCommand(
            Func<object, Task> executeDelegate = null,
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
