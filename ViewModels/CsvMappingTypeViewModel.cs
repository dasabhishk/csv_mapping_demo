using CommunityToolkit.Mvvm.ComponentModel;
using CsvMapper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CsvMapper.ViewModels
{
    /// <summary>
    /// View model for a specific CSV mapping type (PatientStudy or SeriesInstance)
    /// </summary>
    public partial class CsvMappingTypeViewModel : ObservableObject
    {
        /// <summary>
        /// The type of CSV file (PatientStudy or SeriesInstance)
        /// </summary>
        [ObservableProperty]
        private string _csvType = string.Empty;
        
        /// <summary>
        /// Name of the associated database table
        /// </summary>
        [ObservableProperty]
        private string _tableName = string.Empty;
        
        /// <summary>
        /// Path to the CSV file being mapped
        /// </summary>
        [ObservableProperty]
        private string _csvFilePath = string.Empty;
        
        /// <summary>
        /// Flag to indicate if the CSV file has been loaded
        /// </summary>
        [ObservableProperty]
        private bool _isCsvLoaded;
        
        /// <summary>
        /// List of column mappings for this CSV type
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ColumnMappingViewModel> _columnMappings = new();
        
        /// <summary>
        /// The database table schema
        /// </summary>
        [ObservableProperty]
        private SchemaTable _schemaTable = new();
        
        /// <summary>
        /// List of CSV columns loaded from the CSV file
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CsvColumn> _csvColumns = new();
        
        /// <summary>
        /// Flag indicating if this mapping is complete and valid
        /// </summary>
        [ObservableProperty]
        private bool _isValid;
        
        /// <summary>
        /// Flag indicating if this mapping type has been completed
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;
    }
}