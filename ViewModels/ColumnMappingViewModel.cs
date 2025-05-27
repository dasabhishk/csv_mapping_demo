using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using System.Collections.Generic;
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
        
        /// <summary>
        /// Indicates if this column has a transformation applied
        /// </summary>
        [ObservableProperty]
        private bool _hasTransformation = false;
        
        /// <summary>
        /// Type of transformation applied, if any
        /// </summary>
        [ObservableProperty]
        private TransformationType? _transformationType;
        
        /// <summary>
        /// Parameters for the transformation, if any
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, object> _transformationParameters = new();
        
        /// <summary>
        /// Display name for the transformation
        /// </summary>
        [ObservableProperty]
        private string _transformationDisplayName = string.Empty;
        
        /// <summary>
        /// Available transformation types for this mapping
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<TransformationType> _availableTransformations = new();
        
        /// <summary>
        /// Indicates whether transformation should be allowed for this column
        /// </summary>
        [ObservableProperty]
        private bool _canBeTransformed;
        
        /// <summary>
        /// Original (pre-transformation) sample values
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _originalSampleValues = new();
        
        /// <summary>
        /// Command to open the transformation dialog
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanOpenTransformationDialog))]
        private void OpenTransformationDialog()
        {
            // This will be implemented to show the transformation dialog
        }
        
        /// <summary>
        /// Determines if the transformation dialog can be opened
        /// </summary>
        /// <returns>True if the column can be transformed based on schema definition</returns>
        private bool CanOpenTransformationDialog()
        {
            // Only allow transformations for columns that are marked as transformable in the schema
            return CanBeTransformed;
        }
        
        /// <summary>
        /// Command to clear the current transformation
        /// </summary>
        [RelayCommand]
        private void ClearTransformation()
        {
            HasTransformation = false;
            TransformationType = null;
            TransformationParameters.Clear();
            TransformationDisplayName = string.Empty;
            
            // Restore original sample values
            SampleValues = new ObservableCollection<string>(OriginalSampleValues);
        }
        
        /// <summary>
        /// Updates available transformations based on the mapping context
        /// </summary>
        public void UpdateAvailableTransformations()
        {
            AvailableTransformations.Clear();
            
            if (string.IsNullOrEmpty(SelectedCsvColumn))
                return;
                
            // Add available transformations based on column types
            
            // For FirstName or LastName columns, add name splitting
            if (DbColumn.Name.Contains("FirstName") || DbColumn.Name.Contains("LastName"))
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.SplitFirstToken);
                AvailableTransformations.Add(Models.Transformations.TransformationType.SplitLastToken);
            }
            
            // For date columns, add date formatting
            if (DbColumn.DataType == "datetime")
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.DateFormat);
            }
            
            // For gender or category columns, add category mapping
            if (DbColumn.Name.Contains("Gender") || DbColumn.Name.Contains("Code"))
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.CategoryMapping);
            }
            
            // For year extraction from dates
            if (DbColumn.Name.Contains("Year") && DbColumn.DataType == "int")
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.DateFormat);
            }
        }
    }
}
