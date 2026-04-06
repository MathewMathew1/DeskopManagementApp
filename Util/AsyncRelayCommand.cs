using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModernWpfApp
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                _isExecuting = value;
                // Tells the UI to re-evaluate if the button should be enabled
                CommandManager.InvalidateRequerySuggested(); 
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return !IsExecuting; // Button is disabled while executing!
        }

        public async void Execute(object? parameter)
        {
            IsExecuting = true;
            try
            {
                await _execute();
            }
            finally
            {
                IsExecuting = false;
            }
        }
    }
}