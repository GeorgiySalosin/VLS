using System.Windows;
using VLSGame.ViewModels;

namespace VLSGame.Views
{
    /// <summary>
    /// Логика взаимодействия для Lobby.xaml
    /// </summary>
    public partial class Lobby : Window
    {
        private readonly LobbyViewModel viewModel;
        public Lobby()
        {
            InitializeComponent();

            viewModel = new LobbyViewModel();
            DataContext = viewModel;
        }
    }
}
