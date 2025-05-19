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
        private string _selectedCsvFilePath = string.Empty;

        [ObservableProperty]
        private string _selectedSchemaFilePath = "stagingdb_details.json";

        [ObservableProperty]
        private bool _isCsvLoaded;

        [ObservableProperty]
        private bool _isSchemaLoaded;

        [ObservableProperty]
        private string _selectedTableName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _availableTables = new();

        [ObservableProperty]
        private ObservableCollection<ColumnMappingViewModel> _columnMappings = new();

        [ObservableProperty]
        private bool _canSaveMapping;

        // Private backing fields
        private List<CsvColumn> _csvColumns = new();
        private DatabaseSchema _databaseSchema = new();
        private SchemaTable? _selectedTable;

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
        /// Command to browse and select a CSV file
        /// </summary>
        [RelayCommand]
        private async Task BrowseCsvFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedCsvFilePath = openFileDialog.FileName;
                await LoadCsvFile();
            }
        }

        /// <summary>
        /// Command to load the CSV file
        /// </summary>
        [RelayCommand]
        private async Task LoadCsvFile()
        {
            if (string.IsNullOrEmpty(SelectedCsvFilePath) || !File.Exists(SelectedCsvFilePath))
            {
                StatusMessage = "Please select a valid CSV file.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Loading CSV file...";

                _csvColumns = await _csvParserService.ParseCsvFileAsync(SelectedCsvFilePath);
                IsCsvLoaded = _csvColumns.Count > 0;

                if (IsCsvLoaded)
                {
                    StatusMessage = $"CSV loaded: {_csvColumns.Count} columns found.";
                    
                    // If schema is already loaded, update the mappings
                    if (IsSchemaLoaded && _selectedTable != null)
                    {
                        UpdateColumnMappings();
                    }
                }
                else
                {
                    StatusMessage = "Failed to load columns from CSV file.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading CSV: {ex.Message}";
                IsCsvLoaded = false;
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
                    
                    // Populate available tables
                    AvailableTables.Clear();
                    foreach (var table in _databaseSchema.Tables)
                    {
                        AvailableTables.Add(table.TableName);
                    }

                    // Select the first table by default if available
                    if (AvailableTables.Count > 0)
                    {
                        SelectedTableName = AvailableTables[0];
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
        /// Updates the column mappings when the selected table changes
        /// </summary>
        partial void OnSelectedTableNameChanged(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsSchemaLoaded)
                return;

            _selectedTable = _databaseSchema.Tables.FirstOrDefault(t => t.TableName == value);
            
            if (_selectedTable != null && IsCsvLoaded)
            {
                UpdateColumnMappings();
            }
            else
            {
                ColumnMappings.Clear();
            }
        }

        /// <summary>
        /// Updates the column mappings based on the selected table and CSV columns
        /// </summary>
        private void UpdateColumnMappings()
        {
            if (_selectedTable == null || _csvColumns.Count == 0)
                return;

            ColumnMappings.Clear();

            // Try to auto-match columns
            var autoMatches = _mappingService.AutoMatchColumns(_csvColumns, _selectedTable.Columns);

            // Create view models for each database column
            foreach (var dbColumn in _selectedTable.Columns)
            {
                var viewModel = new ColumnMappingViewModel
                {
                    DbColumn = dbColumn,
                    AvailableCsvColumns = new ObservableCollection<string>(_csvColumns.Select(c => c.Name))
                };

                // Set auto-matched column if available
                if (autoMatches.TryGetValue(dbColumn.Name, out var matchedCsvColumn))
                {
                    viewModel.SelectedCsvColumn = matchedCsvColumn;
                }

                // Add sample values if a CSV column is selected
                UpdateColumnMappingSampleValues(viewModel);

                // Subscribe to property changed event to update sample values when selection changes
                viewModel.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(ColumnMappingViewModel.SelectedCsvColumn))
                    {
                        UpdateColumnMappingSampleValues(viewModel);
                        ValidateMappings();
                    }
                };

                ColumnMappings.Add(viewModel);
            }

            ValidateMappings();
        }

        /// <summary>
        /// Updates the sample values for a column mapping
        /// </summary>
        private void UpdateColumnMappingSampleValues(ColumnMappingViewModel mappingVm)
        {
            if (string.IsNullOrEmpty(mappingVm.SelectedCsvColumn))
            {
                mappingVm.SampleValues = new ObservableCollection<string>();
                mappingVm.InferredType = string.Empty;
                return;
            }

            var csvColumn = _csvColumns.FirstOrDefault(c => c.Name == mappingVm.SelectedCsvColumn);
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
            if (!ValidateMappings())
            {
                MessageBox.Show("Please fix the validation errors before saving the mapping.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    // Create the mapping result
                    var mappingResult = new MappingResult
                    {
                        TableName = SelectedTableName,
                        ColumnMappings = ColumnMappings
                            .Where(m => !string.IsNullOrEmpty(m.SelectedCsvColumn))
                            .Select(m => new ColumnMapping
                            {
                                CsvColumn = m.SelectedCsvColumn,
                                DbColumn = m.DbColumn.Name
                            })
                            .ToList()
                    };

                    bool success = await _mappingService.SaveMappingsAsync(mappingResult, saveFileDialog.FileName);

                    if (success)
                    {
                        StatusMessage = $"Mappings saved successfully to {saveFileDialog.FileName}";
                        MessageBox.Show("Mappings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        /// Command to auto-match columns
        /// </summary>
        [RelayCommand]
        private void AutoMatchColumns()
        {
            if (_selectedTable == null || _csvColumns.Count == 0)
            {
                StatusMessage = "Both CSV and schema must be loaded to auto-match columns.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Auto-matching columns...";

                var autoMatches = _mappingService.AutoMatchColumns(_csvColumns, _selectedTable.Columns);
                
                // Apply auto-matches to the column mappings
                foreach (var mapping in ColumnMappings)
                {
                    if (autoMatches.TryGetValue(mapping.DbColumn.Name, out var matchedCsvColumn))
                    {
                        mapping.SelectedCsvColumn = matchedCsvColumn;
                    }
                }

                ValidateMappings();
                StatusMessage = $"Auto-matched {autoMatches.Count} columns.";
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
        /// Validates all column mappings
        /// </summary>
        /// <returns>True if all mappings are valid</returns>
        private bool ValidateMappings()
        {
            if (_selectedTable == null || ColumnMappings.Count == 0)
                return false;

            var errors = _mappingService.ValidateMappings(
                ColumnMappings.ToList(), 
                _csvColumns, 
                _selectedTable.Columns);

            // Update validation errors in view models
            foreach (var mapping in ColumnMappings)
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

            // Update the CanSaveMapping property
            CanSaveMapping = errors.Count == 0 && ColumnMappings.All(m => !string.IsNullOrEmpty(m.SelectedCsvColumn));
            
            return errors.Count == 0;
        }
    }
}
