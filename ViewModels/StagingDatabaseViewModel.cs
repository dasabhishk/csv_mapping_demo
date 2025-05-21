using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace CsvMapper.ViewModels
{
    public partial class StagingDatabaseViewModel : ObservableObject
    {
        [ObservableProperty] private string serverName = "";
        [ObservableProperty] private string databaseName = "";
        [ObservableProperty] private string username = "";
        [ObservableProperty] private string password = "";
        [ObservableProperty] private string selectedAuthType = "SQL Authentication";

        public ObservableCollection<string> AuthenticationTypes { get; } = new()
        {
            "SQL Authentication",
            "Windows Authentication"
        };

        [RelayCommand]
        private void Connect()
        {
            MessageBox.Show($"Connecting to {ServerName}\\{DatabaseName} using {SelectedAuthType}");
        }
    }
}
