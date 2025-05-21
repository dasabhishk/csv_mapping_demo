using CsvMapper.ViewModels;
using System.Windows.Controls;

namespace CsvMapper.Views
{
    /// <summary>
    /// Interaction logic for MappingView.xaml
    /// </summary>
    public partial class MappingView : UserControl
    {
        public MappingView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the view model when the control is loaded
        /// </summary>
        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeCommand.ExecuteAsync(null);
            }
        }
    }
}
