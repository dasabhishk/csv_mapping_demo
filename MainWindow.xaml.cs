using CsvMapper.ViewModels;
using System.Windows;

namespace CsvMapper
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
