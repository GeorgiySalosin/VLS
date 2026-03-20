using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using VLSGame.ViewModels;

using VLSShared.Models;

namespace VLSGame
{
    public partial class MainWindow : Window
    {
        private readonly LobbyViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();


            _viewModel = new LobbyViewModel();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.MessageAdded += OnMessageAdded;

            UpdateUIFromViewModel();
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(LobbyViewModel.ConnectionStatus):
                    case nameof(LobbyViewModel.ConnectionStatusColor):
                        UpdateConnectionStatus();
                        break;
                    case nameof(LobbyViewModel.LastResponse):
                        LastResponseText.Text = _viewModel.LastResponse;
                        break;
                }
            });
        }

        private void OnMessageAdded(object? sender, string message)
        {
            Dispatcher.Invoke(() => AddMessage(message));
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatusText.Text = _viewModel.ConnectionStatus;
            ConnectionStatusText.Foreground = _viewModel.ConnectionStatusColor;
            ConnectButton.Content = _viewModel.ConnectionStatus == "Connected" ? "Disconnect" : "Connect";
        }

        private void UpdateUIFromViewModel()
        {
            ServerIpTextBox.Text = _viewModel.ServerIp;
            ServerPortTextBox.Text = _viewModel.ServerPort;
            UpdateConnectionStatus();
            LastResponseText.Text = _viewModel.LastResponse;

            foreach (var message in _viewModel.Messages)
            {
                AddMessage(message);
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ServerIp = ServerIpTextBox.Text;
            _viewModel.ServerPort = ServerPortTextBox.Text;
            await _viewModel.ConnectAsync();
        }

        private async void ClickArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_viewModel.IsConnected)
            {
                MessageBox.Show("No server connection", "Warning",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var position = e.GetPosition(sender as IInputElement);
            await _viewModel.SendMouseClickAsync(position.X, position.Y);
        }

        private async void OpenPanoramaButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите HDRI панораму",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.hdr;*.exr|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                await _viewModel.StartSinglePlayerAsync(dialog.FileName);

                // Открываем окно Match
                var matchViewModel = new MatchViewModel(_viewModel.CurrentGameMode!, dialog.FileName);
                var matchWindow = new Match(matchViewModel);
                matchWindow.Show();
                this.Close();
            }
        }

        private void AddMessage(string message)
        {
            MessagesListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            while (MessagesListBox.Items.Count > 50)
            {
                MessagesListBox.Items.RemoveAt(MessagesListBox.Items.Count - 1);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.MessageAdded -= OnMessageAdded;
            _viewModel.Disconnect();
            base.OnClosed(e);
        }
    }
}