using CommunityToolkit.Mvvm.ComponentModel;

namespace CsvMapper.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndex = 0;
    }
}

