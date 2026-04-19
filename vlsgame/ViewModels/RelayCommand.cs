using System.Windows.Input;
using VLSGame.Models;

namespace VLSGame.ViewModels
{
    internal class RelayCommand<T> : ICommand
    {
        private Action<MapButtonData> onSelectMap;

        public RelayCommand(Action<MapButtonData> onSelectMap)
        {
            this.onSelectMap = onSelectMap;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}