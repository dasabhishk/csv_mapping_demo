using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using System;
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
        [NotifyCanExecuteChangedFor(nameof(OpenTransformationDialogCommand))]
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
        /// Validation warning message for this mapping
        /// </summary>
        [ObservableProperty]
        private string _validationWarning = string.Empty;
        
        /// <summary>
        /// Event that requests opening the transformation dialog
        /// This is handled by the view to show the actual dialog
        /// </summary>
        public event EventHandler? OpenTransformationDialogRequested;
        

        
        /// <summary>
        /// Command to open the transformation dialog
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanOpenTransformationDialog))]
        private void OpenTransformationDialog()
        {
            // Raise event for the view to handle
            OpenTransformationDialogRequested?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Determines if the transformation dialog can be opened
        /// </summary>
        /// <returns>True if the column can be transformed based on schema definition</returns>
        private bool CanOpenTransformationDialog()
        {
            // Only allow transformations for columns that are marked as transformable in the schema
            return CanBeTransformed && !string.IsNullOrEmpty(SelectedCsvColumn);
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
        /// Applies a transformation to this column mapping
        /// </summary>
        /// <param name="transformation">The transformation to apply</param>
        public void ApplyTransformation(ITransformation transformation)
        {
            ApplyTransformation(transformation, new Dictionary<string, object>());
        }
        
        /// <summary>
        /// Applies a transformation to this column mapping with parameters
        /// </summary>
        /// <param name="transformation">The transformation to apply</param>
        /// <param name="parameters">Parameters for the transformation</param>
        public void ApplyTransformation(ITransformation transformation, Dictionary<string, object> parameters)
        {
            if (transformation == null)
                return;
                
            HasTransformation = true;
            TransformationType = transformation.Type;
            TransformationDisplayName = GetTransformationDisplayName(transformation);
            
            // Store transformation parameters from UI
            TransformationParameters.Clear();
            foreach (var param in parameters)
            {
                TransformationParameters[param.Key] = param.Value;
            }
            
            // Apply transformation to sample values with parameters
            var transformedSamples = new List<string>();
            foreach (var sample in OriginalSampleValues)
            {
                try
                {
                    var transformed = transformation.Transform(sample, parameters);
                    transformedSamples.Add(transformed);
                }
                catch
                {
                    transformedSamples.Add($"[Error transforming: {sample}]");
                }
            }
            
            SampleValues = new ObservableCollection<string>(transformedSamples);
        }
        
        /// <summary>
        /// Gets display name for a transformation
        /// </summary>
        private string GetTransformationDisplayName(ITransformation transformation)
        {
            return transformation.Type switch
            {
                Models.Transformations.TransformationType.SplitFirstToken => "Extract First Word",
                Models.Transformations.TransformationType.SplitLastToken => "Extract Last Word",
                Models.Transformations.TransformationType.DateFormat => "Format Date",
                Models.Transformations.TransformationType.CategoryMapping => "Map Categories",
                _ => transformation.Type.ToString()
            };
        }
        
        /// <summary>
        /// Updates available transformations based on the mapping context
        /// </summary>
        public void UpdateAvailableTransformations()
        {
            AvailableTransformations.Clear();
            
            if (string.IsNullOrEmpty(SelectedCsvColumn) || SelectedCsvColumn == "-- No Mapping (Optional) --")
                return;
                
            if (!CanBeTransformed)
                return;
                
            // Use unified logic for determining available transformations
            var columnName = DbColumn.Name.ToLower();
            
            // Name fields can be split
            if (columnName.Contains("name"))
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.SplitFirstToken);
                AvailableTransformations.Add(Models.Transformations.TransformationType.SplitLastToken);
            }
            
            // Date fields can be formatted
            if (columnName.Contains("date") || columnName.Contains("time") || columnName.Contains("year") || 
                DbColumn.DataType?.ToLower() == "datetime" || DbColumn.DataType?.ToLower() == "date")
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.DateFormat);
            }
            
            // Gender and other categorical fields can be mapped
            if (columnName.Contains("gender") || columnName.Contains("status") || columnName.Contains("type") || 
                columnName.Contains("code") || columnName.Contains("category"))
            {
                AvailableTransformations.Add(Models.Transformations.TransformationType.CategoryMapping);
            }
        }
    }
}
