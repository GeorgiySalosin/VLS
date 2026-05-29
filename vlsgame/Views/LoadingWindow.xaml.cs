using System.Windows;
using VLSGame.ViewModels;

namespace VLSGame.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        internal readonly LoadingWindowViewModel viewModel;
        public LoadingWindow()
        {
            InitializeComponent();

            viewModel = new LoadingWindowViewModel();
            DataContext = viewModel;
        }
    }
}
