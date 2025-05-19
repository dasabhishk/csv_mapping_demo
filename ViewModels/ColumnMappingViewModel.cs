using CommunityToolkit.Mvvm.ComponentModel;
using CsvMapper.Models;
using System.Collections.ObjectModel;

namespace CsvMapper.ViewModels
{
    /// <summary>
    /// View model for individual column mappings
    /// </summary>
    public partial class ColumnMappingViewModel : ObservableObject
    {
        /// <summary>
        /// Database column definition
        /// </summary>
        [ObservableProperty]
        private DatabaseColumn _dbColumn = new();

        /// <summary>
        /// Selected CSV column name
        /// </summary>
        [ObservableProperty]
        private string _selectedCsvColumn = string.Empty;

        /// <summary>
        /// Available CSV columns to choose from
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _availableCsvColumns = new();

        /// <summary>
        /// Sample values from the selected CSV column
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _sampleValues = new();

        /// <summary>
        /// Inferred type from the selected CSV column
        /// </summary>
        [ObservableProperty]
        private string _inferredType = string.Empty;

        /// <summary>
        /// Validation error message, if any
        /// </summary>
        [ObservableProperty]
        private string _validationError = string.Empty;

        /// <summary>
        /// Indicates if the mapping is valid
        /// </summary>
        [ObservableProperty]
        private bool _isValid = true;
    }
}
