using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvMapper.Helpers;
using CsvMapper.Models;
using CsvMapper.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CsvMapper.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ICsvParserService _csvParserService;
        private readonly ISchemaLoaderService _schemaLoaderService;
        private readonly IMappingService _mappingService;

        // Observable properties for binding
        [ObservableProperty]
        private string _statusMessage = "Ready to map CSV to database schema.";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _selectedSchemaFilePath = string.Empty;

        [ObservableProperty]
        private bool _isSchemaLoaded;

        [ObservableProperty]
        private ObservableCollection<string> _availableCsvTypes = new();

        [ObservableProperty]
        private string _selectedCsvType = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ColumnMappingViewModel> _columnMappings = new();

        [ObservableProperty]
        private bool _canSaveMapping;

        [ObservableProperty]
        private ObservableCollection<CsvMappingTypeViewModel> _mappingTypes = new();

        [ObservableProperty]
        private CsvMappingTypeViewModel? _currentMappingType;

        [ObservableProperty]
        private bool _canAddNewMapping = true;

        [ObservableProperty]
        private MultiMappingResult _savedMappings = new();

        // Private backing fields
        private DatabaseSchema _databaseSchema = new();

        /// <summary>
        /// Constructor with injected dependencies
        /// </summary>
        public MainViewModel(
            ICsvParserService csvParserService,
            ISchemaLoaderService schemaLoaderService,
            IMappingService mappingService)
        {
            _csvParserService = csvParserService;
            _schemaLoaderService = schemaLoaderService;
            _mappingService = mappingService;
        }

        /// <summary>
        /// Command to initialize the application
        /// </summary>
        [RelayCommand]
        private async Task Initialize()
        {
            await LoadSchemaFile();
            
            // Try to load existing mappings
            try
            {
                var existingMappings = await _mappingService.LoadMappingsAsync("mappings.json");
                if (existingMappings != null && existingMappings.Mappings.Count > 0)
                {
                    SavedMappings = existingMappings;
                    StatusMessage = $"Loaded {existingMappings.Mappings.Count} existing mappings.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Note: Could not load existing mappings. Starting fresh. ({ex.Message})";
            }
        }

        /// <summary>
        /// Command to browse and select a CSV file for the current mapping type
        /// </summary>
        [RelayCommand]
        private async Task BrowseCsvFile()
        {
            if (CurrentMappingType == null)
            {
                MessageBox.Show("Please select a CSV type first.", "CSV Type Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = $"Select a CSV file for {CurrentMappingType.CsvType}"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentMappingType.CsvFilePath = openFileDialog.FileName;
                await LoadCsvFile(CurrentMappingType);
            }
        }

        /// <summary>
        /// Load a CSV file for a specific mapping type
        /// </summary>
        private async Task LoadCsvFile(CsvMappingTypeViewModel mappingType)
        {
            if (string.IsNullOrEmpty(mappingType.CsvFilePath) || !File.Exists(mappingType.CsvFilePath))
            {
                StatusMessage = "Please select a valid CSV file.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Loading CSV file for {mappingType.CsvType}...";

                mappingType.CsvColumns = await _csvParserService.ParseCsvFileAsync(mappingType.CsvFilePath);
                mappingType.IsCsvLoaded = mappingType.CsvColumns.Count > 0;

                if (mappingType.IsCsvLoaded)
                {
                    StatusMessage = $"CSV loaded for {mappingType.CsvType}: {mappingType.CsvColumns.Count} columns found.";
                    
                    // Update the mappings
                    UpdateColumnMappings(mappingType);
                }
                else
                {
                    StatusMessage = $"Failed to load columns from CSV file for {mappingType.CsvType}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading CSV: {ex.Message}";
                mappingType.IsCsvLoaded = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Command to browse and select a schema JSON file
        /// </summary>
        [RelayCommand]
        private async Task BrowseSchemaFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Select a schema JSON file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedSchemaFilePath = openFileDialog.FileName;
                await LoadSchemaFile();
            }
        }

        /// <summary>
        /// Command to load the schema file
        /// </summary>
        [RelayCommand]
        private async Task LoadSchemaFile()
        {
            if (string.IsNullOrEmpty(SelectedSchemaFilePath) || !File.Exists(SelectedSchemaFilePath))
            {
                StatusMessage = "Please select a valid schema JSON file.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Loading schema file...";

                _databaseSchema = await _schemaLoaderService.LoadSchemaAsync(SelectedSchemaFilePath);
                IsSchemaLoaded = _databaseSchema.Tables.Count > 0;

                if (IsSchemaLoaded)
                {
                    StatusMessage = $"Schema loaded: {_databaseSchema.Tables.Count} tables found.";
                    
                    // Populate available CSV types
                    AvailableCsvTypes.Clear();
                    foreach (var table in _databaseSchema.Tables)
                    {
                        AvailableCsvTypes.Add(table.CsvType);
                    }

                    // Select the first type by default if available
                    if (AvailableCsvTypes.Count > 0)
                    {
                        SelectedCsvType = AvailableCsvTypes[0];
                    }
                }
                else
                {
                    StatusMessage = "Failed to load tables from schema file.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading schema: {ex.Message}";
                IsSchemaLoaded = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Updates the current mapping type when the selected CSV type changes
        /// </summary>
        partial void OnSelectedCsvTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsSchemaLoaded)
                return;

            // Find the associated table schema
            var tableSchema = _databaseSchema.Tables.FirstOrDefault(t => t.CsvType == value);
            if (tableSchema == null)
                return;

            // Check if we already have a mapping for this type
            var existingMapping = MappingTypes.FirstOrDefault(m => m.CsvType == value);
            if (existingMapping != null)
            {
                CurrentMappingType = existingMapping;
                ColumnMappings = existingMapping.ColumnMappings;
                return;
            }

            // Create a new mapping type
            var newMappingType = new CsvMappingTypeViewModel
            {
                CsvType = value,
                TableName = tableSchema.TableName,
                SchemaTable = tableSchema
            };

            MappingTypes.Add(newMappingType);
            CurrentMappingType = newMappingType;
            ColumnMappings = newMappingType.ColumnMappings;

            // Check if we have a saved mapping for this type
            var savedMapping = SavedMappings.Mappings.FirstOrDefault(m => m.CsvType == value);
            if (savedMapping != null)
            {
                StatusMessage = $"Found existing mapping for {value}. Loading...";
                LoadSavedMapping(savedMapping, newMappingType);
            }
        }

        /// <summary>
        /// Loads a saved mapping into a mapping type view model
        /// </summary>
        private void LoadSavedMapping(MappingResult savedMapping, CsvMappingTypeViewModel mappingType)
        {
            // We'll populate the mappings once the CSV file is loaded
            mappingType.IsCompleted = true;
        }

        /// <summary>
        /// Updates the column mappings based on the selected table and CSV columns
        /// </summary>
        private void UpdateColumnMappings(CsvMappingTypeViewModel mappingType)
        {
            if (mappingType.SchemaTable == null || mappingType.CsvColumns.Count == 0)
                return;

            mappingType.ColumnMappings.Clear();

            // Try to auto-match columns
            var autoMatches = _mappingService.AutoMatchColumns(mappingType.CsvColumns, mappingType.SchemaTable.Columns);

            // Create view models for each database column
            foreach (var dbColumn in mappingType.SchemaTable.Columns)
            {
                var viewModel = new ColumnMappingViewModel
                {
                    DbColumn = dbColumn,
                    AvailableCsvColumns = new ObservableCollection<string>(mappingType.CsvColumns.Select(c => c.Name))
                };

                // Set auto-matched column if available
                if (autoMatches.TryGetValue(dbColumn.Name, out var matchedCsvColumn))
                {
                    viewModel.SelectedCsvColumn = matchedCsvColumn;
                }

                // Add sample values if a CSV column is selected
                UpdateColumnMappingSampleValues(viewModel, mappingType);

                // Subscribe to property changed event to update sample values when selection changes
                viewModel.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(ColumnMappingViewModel.SelectedCsvColumn))
                    {
                        UpdateColumnMappingSampleValues(viewModel, mappingType);
                        ValidateMappings(mappingType);
                    }
                };

                mappingType.ColumnMappings.Add(viewModel);
            }

            // If this is the current mapping type, update the UI
            if (mappingType == CurrentMappingType)
            {
                ColumnMappings = mappingType.ColumnMappings;
            }

            ValidateMappings(mappingType);

            // Check for saved mappings
            var savedMapping = SavedMappings.Mappings.FirstOrDefault(m => m.CsvType == mappingType.CsvType);
            if (savedMapping != null && mappingType.IsCompleted)
            {
                ApplySavedMapping(savedMapping, mappingType);
            }
        }

        /// <summary>
        /// Applies a saved mapping to the mapping type
        /// </summary>
        private void ApplySavedMapping(MappingResult savedMapping, CsvMappingTypeViewModel mappingType)
        {
            foreach (var mapping in savedMapping.ColumnMappings)
            {
                var viewModel = mappingType.ColumnMappings.FirstOrDefault(
                    vm => vm.DbColumn.Name == mapping.DbColumn);
                
                if (viewModel != null && mappingType.CsvColumns.Any(c => c.Name == mapping.CsvColumn))
                {
                    viewModel.SelectedCsvColumn = mapping.CsvColumn;
                }
            }

            ValidateMappings(mappingType);
        }

        /// <summary>
        /// Updates the sample values for a column mapping
        /// </summary>
        private void UpdateColumnMappingSampleValues(ColumnMappingViewModel mappingVm, CsvMappingTypeViewModel mappingType)
        {
            if (string.IsNullOrEmpty(mappingVm.SelectedCsvColumn))
            {
                mappingVm.SampleValues = new ObservableCollection<string>();
                mappingVm.InferredType = string.Empty;
                return;
            }

            var csvColumn = mappingType.CsvColumns.FirstOrDefault(c => c.Name == mappingVm.SelectedCsvColumn);
            if (csvColumn != null)
            {
                mappingVm.SampleValues = new ObservableCollection<string>(csvColumn.SampleValues);
                mappingVm.InferredType = csvColumn.InferredType;
            }
            else
            {
                mappingVm.SampleValues = new ObservableCollection<string>();
                mappingVm.InferredType = string.Empty;
            }
        }

        /// <summary>
        /// Command to save the mappings to a JSON file
        /// </summary>
        [RelayCommand]
        private async Task SaveMappings()
        {
            // Check if we have any valid mappings
            var completedMappings = MappingTypes.Where(m => m.IsValid).ToList();
            if (completedMappings.Count == 0)
            {
                MessageBox.Show("No valid mappings to save. Please fix validation errors first.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Save mapping file",
                FileName = "mappings.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Saving mappings...";

                    // Create the multi mapping result
                    var multiMappingResult = new MultiMappingResult
                    {
                        Mappings = completedMappings.Select(m => new MappingResult
                        {
                            TableName = m.TableName,
                            CsvType = m.CsvType,
                            ColumnMappings = m.ColumnMappings
                                .Where(c => !string.IsNullOrEmpty(c.SelectedCsvColumn))
                                .Select(c => new ColumnMapping
                                {
                                    CsvColumn = c.SelectedCsvColumn,
                                    DbColumn = c.DbColumn.Name
                                })
                                .ToList()
                        }).ToList()
                    };

                    bool success = await _mappingService.SaveMultiMappingsAsync(multiMappingResult, saveFileDialog.FileName);

                    if (success)
                    {
                        SavedMappings = multiMappingResult;
                        StatusMessage = $"Mappings saved successfully to {saveFileDialog.FileName}";
                        MessageBox.Show($"Saved {multiMappingResult.Mappings.Count} mappings successfully.", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "Failed to save mappings.";
                        MessageBox.Show("Failed to save mappings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error saving mappings: {ex.Message}";
                    MessageBox.Show($"Error saving mappings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Command to auto-match columns for the current mapping type
        /// </summary>
        [RelayCommand]
        private void AutoMatchColumns()
        {
            if (CurrentMappingType == null || !CurrentMappingType.IsCsvLoaded)
            {
                StatusMessage = "No CSV file loaded for this mapping type.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Auto-matching columns for {CurrentMappingType.CsvType}...";

                var autoMatches = _mappingService.AutoMatchColumns(
                    CurrentMappingType.CsvColumns, 
                    CurrentMappingType.SchemaTable.Columns);
                
                // Apply auto-matches to the column mappings
                foreach (var mapping in CurrentMappingType.ColumnMappings)
                {
                    if (autoMatches.TryGetValue(mapping.DbColumn.Name, out var matchedCsvColumn))
                    {
                        mapping.SelectedCsvColumn = matchedCsvColumn;
                    }
                }

                ValidateMappings(CurrentMappingType);
                StatusMessage = $"Auto-matched {autoMatches.Count} columns for {CurrentMappingType.CsvType}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error during auto-matching: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validates all column mappings for a mapping type
        /// </summary>
        /// <returns>True if all mappings are valid</returns>
        private bool ValidateMappings(CsvMappingTypeViewModel mappingType)
        {
            if (mappingType.SchemaTable == null || mappingType.ColumnMappings.Count == 0)
                return false;

            var errors = _mappingService.ValidateMappings(
                mappingType.ColumnMappings.ToList(), 
                mappingType.CsvColumns, 
                mappingType.SchemaTable.Columns);

            // Update validation errors in view models
            foreach (var mapping in mappingType.ColumnMappings)
            {
                if (errors.TryGetValue(mapping.DbColumn.Name, out var error))
                {
                    mapping.ValidationError = error;
                    mapping.IsValid = false;
                }
                else
                {
                    mapping.ValidationError = string.Empty;
                    mapping.IsValid = !string.IsNullOrEmpty(mapping.SelectedCsvColumn);
                }
            }

            // Update the mapping type validation status
            mappingType.IsValid = errors.Count == 0 && 
                mappingType.ColumnMappings.All(m => !string.IsNullOrEmpty(m.SelectedCsvColumn));

            // If this is the current mapping type, update the UI
            if (mappingType == CurrentMappingType)
            {
                CanSaveMapping = mappingType.IsValid;
            }
            
            // Check if we have valid mappings for both types
            UpdateCanAddNewMapping();
            
            return errors.Count == 0;
        }

        /// <summary>
        /// Updates the CanAddNewMapping flag based on the state of existing mappings
        /// </summary>
        private void UpdateCanAddNewMapping()
        {
            // Count how many of each CSV type we have
            int patientStudyCount = MappingTypes.Count(m => m.CsvType == "PatientStudy");
            int seriesInstanceCount = MappingTypes.Count(m => m.CsvType == "SeriesInstance");

            // We only allow one of each type
            CanAddNewMapping = patientStudyCount < 1 || seriesInstanceCount < 1;
        }

        /// <summary>
        /// Command to switch to a different mapping type
        /// </summary>
        [RelayCommand]
        private void SwitchMappingType(CsvMappingTypeViewModel mappingType)
        {
            if (mappingType != null)
            {
                CurrentMappingType = mappingType;
                SelectedCsvType = mappingType.CsvType;
                ColumnMappings = mappingType.ColumnMappings;
                StatusMessage = $"Switched to {mappingType.CsvType} mapping.";
            }
        }

        /// <summary>
        /// Command to remove a mapping type
        /// </summary>
        [RelayCommand]
        private void RemoveMappingType(CsvMappingTypeViewModel mappingType)
        {
            if (mappingType != null)
            {
                MappingTypes.Remove(mappingType);
                UpdateCanAddNewMapping();

                if (CurrentMappingType == mappingType)
                {
                    // Select another mapping type if available
                    if (MappingTypes.Count > 0)
                    {
                        SwitchMappingType(MappingTypes[0]);
                    }
                    else
                    {
                        CurrentMappingType = null;
                        ColumnMappings.Clear();
                    }
                }

                StatusMessage = $"Removed {mappingType.CsvType} mapping.";
            }
        }

        /// <summary>
        /// Command to initialize a new application session
        /// </summary>
        [RelayCommand]
        private async Task InitializeNewSession()
        {
            var result = MessageBox.Show(
                "This will clear all current mappings. Do you want to continue?",
                "Clear Mappings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MappingTypes.Clear();
                ColumnMappings.Clear();
                CurrentMappingType = null;
                SavedMappings = new MultiMappingResult();
                CanAddNewMapping = true;

                await LoadSchemaFile();
                StatusMessage = "Started new mapping session.";
            }
        }
    }
}
