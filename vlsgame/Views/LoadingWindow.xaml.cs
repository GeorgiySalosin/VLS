using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
