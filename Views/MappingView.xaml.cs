using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using CsvMapper.Services;
using CsvMapper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CsvMapper.Views
{
    /// <summary>
    /// Interaction logic for MappingView.xaml
    /// </summary>
    public partial class MappingView : UserControl
    {
        private readonly ITransformationService _transformationService;
        
        public MappingView()
        {
            InitializeComponent();
            
            // Get transformation service from service locator
            _transformationService = ServiceLocator.TransformationService;
        }

        /// <summary>
        /// Initializes the view model when the control is loaded
        /// </summary>
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeCommand.ExecuteAsync(null);
                
                // Subscribe to transformation dialog events from column mappings
                foreach (var mappingType in viewModel.MappingTypes)
                {
                    SubscribeToColumnMappingEvents(mappingType);
                }
                
                // Subscribe to new mapping types being added
                viewModel.MappingTypes.CollectionChanged += (s, args) =>
                {
                    if (args.NewItems != null)
                    {
                        foreach (CsvMappingTypeViewModel newMappingType in args.NewItems)
                        {
                            SubscribeToColumnMappingEvents(newMappingType);
                        }
                    }
                };
                
                // Subscribe to current mapping type changes to ensure events are wired
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.CurrentMappingType) && viewModel.CurrentMappingType != null)
                    {
                        SubscribeToColumnMappingEvents(viewModel.CurrentMappingType);
                    }
                    // Also subscribe when ColumnMappings collection changes
                    if (args.PropertyName == nameof(MainViewModel.ColumnMappings))
                    {
                        SubscribeToCurrentColumnMappings(viewModel);
                    }
                };
            }
        }

        /// <summary>
        /// Subscribes to transformation dialog events for a mapping type
        /// </summary>
        private void SubscribeToColumnMappingEvents(CsvMappingTypeViewModel mappingType)
        {
            if (mappingType?.ColumnMappings == null) return;
            
            foreach (var columnMapping in mappingType.ColumnMappings)
            {
                // Unsubscribe first to avoid duplicate subscriptions
                columnMapping.OpenTransformationDialogRequested -= OnShowTransformationDialog;
                columnMapping.OpenTransformationDialogRequested += OnShowTransformationDialog;
            }
            
            // Subscribe to new column mappings being added
            mappingType.ColumnMappings.CollectionChanged += (s, args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (ColumnMappingViewModel newColumnMapping in args.NewItems)
                    {
                        newColumnMapping.OpenTransformationDialogRequested -= OnShowTransformationDialog;
                        newColumnMapping.OpenTransformationDialogRequested += OnShowTransformationDialog;
                    }
                }
            };
        }

        /// <summary>
        /// Subscribes to transformation dialog events for current column mappings
        /// </summary>
        private void SubscribeToCurrentColumnMappings(MainViewModel viewModel)
        {
            if (viewModel?.ColumnMappings == null) return;
            
            foreach (var columnMapping in viewModel.ColumnMappings)
            {
                // Unsubscribe first to avoid duplicate subscriptions
                columnMapping.OpenTransformationDialogRequested -= OnShowTransformationDialog;
                columnMapping.OpenTransformationDialogRequested += OnShowTransformationDialog;
            }
        }

        /// <summary>
        /// Handles the transformation dialog event
        /// </summary>
        private void OnShowTransformationDialog(object? sender, EventArgs e)
        {
            if (sender is ColumnMappingViewModel mappingViewModel)
            {
                ShowTransformationDialog(mappingViewModel);
            }
        }

        /// <summary>
        /// Shows the transformation dialog for a column mapping
        /// </summary>
        private void ShowTransformationDialog(ColumnMappingViewModel mappingViewModel)
        {
            // Validate prerequisites
            if (string.IsNullOrEmpty(mappingViewModel.SelectedCsvColumn))
            {
                MessageBox.Show("Please select a CSV column first.", "Column Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mainViewModel = (MainViewModel)DataContext;
            if (mainViewModel?.CurrentMappingType == null)
            {
                MessageBox.Show("No mapping type selected.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show appropriate transformation dialog based on column type
            ShowTransformationConfigurationDialog(mappingViewModel, 
                $"Configure Transformation for {mappingViewModel.SelectedCsvColumn}");
        }

        /// <summary>
        /// Shows the transformation configuration dialog
        /// </summary>
        private void ShowTransformationConfigurationDialog(ColumnMappingViewModel mappingViewModel, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            var scrollViewer = new ScrollViewer();
            var mainPanel = new StackPanel { Margin = new Thickness(20) };

            // Header
            var headerText = new TextBlock
            {
                Text = $"Configure Transformation for: {mappingViewModel.SelectedCsvColumn}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(headerText);

            // Get available transformations for this column
            var availableTransformations = mappingViewModel.AvailableTransformations ?? new ObservableCollection<TransformationType>();
            
            if (!availableTransformations.Any())
            {
                var noTransformText = new TextBlock
                {
                    Text = "No transformations available for this column type.",
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                mainPanel.Children.Add(noTransformText);
            }
            else
            {
                // Transformation Type Selection
                var typeLabel = new TextBlock
                {
                    Text = "Transformation Type:",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                mainPanel.Children.Add(typeLabel);

                var transformationComboBox = new ComboBox
                {
                    Margin = new Thickness(0, 0, 0, 20),
                    Height = 30
                };

                foreach (var transformationType in availableTransformations)
                {
                    transformationComboBox.Items.Add(new ComboBoxItem 
                    { 
                        Content = GetTransformationDisplayName(transformationType, mappingViewModel.DbColumn?.Name ?? ""),
                        Tag = transformationType
                    });
                }

                transformationComboBox.SelectedIndex = 0;
                mainPanel.Children.Add(transformationComboBox);

                // Parameters Panel
                var parametersPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
                mainPanel.Children.Add(parametersPanel);

                // Parameters storage
                var parameters = new Dictionary<string, object>();

                // Function to update parameters panel based on selected transformation
                Action updateParametersPanel = () =>
                {
                    parametersPanel.Children.Clear();
                    if (transformationComboBox.SelectedItem is ComboBoxItem selectedItem && 
                        selectedItem.Tag is TransformationType transformationType)
                    {
                        CreateParametersUI(parametersPanel, transformationType, parameters);
                    }
                };

                // Handle transformation type selection change
                transformationComboBox.SelectionChanged += (s, e) => updateParametersPanel();

                // Initialize parameters panel
                updateParametersPanel();

                // Sample Values Preview
                var previewLabel = new TextBlock
                {
                    Text = "Preview (first 3 sample values):",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                mainPanel.Children.Add(previewLabel);

                var previewPanel = new StackPanel
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    Margin = new Thickness(0, 0, 0, 20),
                    MaxHeight = 100
                };
                mainPanel.Children.Add(previewPanel);

                // Function to update preview
                Action updatePreview = () =>
                {
                    previewPanel.Children.Clear();
                    if (transformationComboBox.SelectedItem is ComboBoxItem selectedItem && 
                        selectedItem.Tag is TransformationType transformationType)
                    {
                        UpdateTransformationPreview(previewPanel, mappingViewModel, transformationType, parameters);
                    }
                };

                // Update preview when parameters change
                transformationComboBox.SelectionChanged += (s, e) => updatePreview();
                updatePreview();

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0)
                };

                var okButton = new Button { Content = "Apply Transformation", Width = 150, Margin = new Thickness(0, 0, 10, 0) };
                var cancelButton = new Button { Content = "Cancel", Width = 80 };

                okButton.Click += (s, e) =>
                {
                    if (transformationComboBox.SelectedItem is ComboBoxItem selectedItem && 
                        selectedItem.Tag is TransformationType transformationType)
                    {
                        try
                        {
                            var transformation = _transformationService.CreateTransformation(transformationType, mappingViewModel.SelectedCsvColumn);
                            
                            // Apply transformation WITH parameters
                            mappingViewModel.ApplyTransformation(transformation, parameters);
                            
                            // Update validation
                            var mainViewModel = (MainViewModel)DataContext;
                            if (mainViewModel?.CurrentMappingType != null)
                            {
                                mainViewModel.ValidateMappings(mainViewModel.CurrentMappingType);
                            }
                            
                            dialog.DialogResult = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error applying transformation: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };
                
                cancelButton.Click += (s, e) => dialog.DialogResult = false;

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                mainPanel.Children.Add(buttonPanel);
            }

            scrollViewer.Content = mainPanel;
            dialog.Content = scrollViewer;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Creates parameter UI controls based on transformation type
        /// </summary>
        private void CreateParametersUI(StackPanel parametersPanel, TransformationType transformationType, Dictionary<string, object> parameters)
        {
            switch (transformationType)
            {
                case TransformationType.SplitFirstToken:
                case TransformationType.SplitLastToken:
                    CreateDelimiterParameterUI(parametersPanel, parameters);
                    break;
                
                case TransformationType.DateFormat:
                    CreateDateFormatParameterUI(parametersPanel, parameters);
                    break;
                
                case TransformationType.CategoryMapping:
                    CreateCategoryMappingParameterUI(parametersPanel, parameters);
                    break;
            }
        }

        /// <summary>
        /// Creates delimiter parameter UI for split transformations
        /// </summary>
        private void CreateDelimiterParameterUI(StackPanel parametersPanel, Dictionary<string, object> parameters)
        {
            var delimiterLabel = new TextBlock
            {
                Text = "Delimiter:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            parametersPanel.Children.Add(delimiterLabel);

            var delimiterComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Height = 25
            };

            // Add common delimiters
            var delimiters = new Dictionary<string, string>
            {
                { "Space", " " },
                { "Comma", "," },
                { "Semicolon", ";" },
                { "Tab", "\t" },
                { "Pipe", "|" },
                { "Hyphen", "-" },
                { "Underscore", "_" }
            };

            foreach (var delimiter in delimiters)
            {
                delimiterComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = delimiter.Key,
                    Tag = delimiter.Value
                });
            }

            delimiterComboBox.SelectedIndex = 0; // Default to space
            delimiterComboBox.SelectionChanged += (s, e) =>
            {
                if (delimiterComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    parameters["Delimiter"] = selectedItem.Tag.ToString();
                }
            };

            parametersPanel.Children.Add(delimiterComboBox);

            var helpText = new TextBlock
            {
                Text = "Select the character to split on",
                FontStyle = FontStyles.Italic,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                Margin = new Thickness(0, 0, 0, 15)
            };
            parametersPanel.Children.Add(helpText);

            // Set initial value
            if (delimiterComboBox.SelectedItem is ComboBoxItem initialItem)
            {
                parameters["Delimiter"] = initialItem.Tag.ToString();
            }
        }

        /// <summary>
        /// Creates date format parameter UI
        /// </summary>
        private void CreateDateFormatParameterUI(StackPanel parametersPanel, Dictionary<string, object> parameters)
        {
            var formatLabel = new TextBlock
            {
                Text = "Target Date Format:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            parametersPanel.Children.Add(formatLabel);

            var formatComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Height = 25
            };

            // Add common date formats
            var dateFormats = new Dictionary<string, string>
            {
                { "ISO8601 (yyyy-MM-dd)", "yyyy-MM-dd" },
                { "US Format (MM/dd/yyyy)", "MM/dd/yyyy" },
                { "European (dd/MM/yyyy)", "dd/MM/yyyy" },
                { "File Friendly (yyyyMMdd)", "yyyyMMdd" },
                { "Long Date (MMMM d, yyyy)", "MMMM d, yyyy" },
                { "Year Only (yyyy)", "yyyy" }
            };

            foreach (var format in dateFormats)
            {
                formatComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = format.Key,
                    Tag = format.Value
                });
            }

            formatComboBox.SelectedIndex = 0;
            formatComboBox.SelectionChanged += (s, e) =>
            {
                if (formatComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    parameters["TargetFormat"] = selectedItem.Tag.ToString();
                }
            };

            parametersPanel.Children.Add(formatComboBox);

            var helpText = new TextBlock
            {
                Text = "Select the desired output format for dates",
                FontStyle = FontStyles.Italic,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                Margin = new Thickness(0, 0, 0, 15)
            };
            parametersPanel.Children.Add(helpText);

            // Set initial value
            if (formatComboBox.SelectedItem is ComboBoxItem initialItem)
            {
                parameters["TargetFormat"] = initialItem.Tag.ToString();
            }
        }

        /// <summary>
        /// Creates category mapping parameter UI
        /// </summary>
        private void CreateCategoryMappingParameterUI(StackPanel parametersPanel, Dictionary<string, object> parameters)
        {
            var mappingLabel = new TextBlock
            {
                Text = "Category Mappings:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            parametersPanel.Children.Add(mappingLabel);

            var helpText = new TextBlock
            {
                Text = "Define how values should be mapped (e.g., Male->M, Female->F)",
                FontStyle = FontStyles.Italic,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            parametersPanel.Children.Add(helpText);

            // For now, provide a simple text area for category mappings
            var mappingTextBox = new TextBox
            {
                Text = "Male=M\nFemale=F\nUnknown=U",
                AcceptsReturn = true,
                Height = 80,
                Margin = new Thickness(0, 0, 0, 15)
            };

            mappingTextBox.TextChanged += (s, e) =>
            {
                // Parse the mapping text and store in parameters
                var mappings = new Dictionary<string, string>();
                var lines = mappingTextBox.Text.Split('\n');
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        mappings[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                parameters["Mappings"] = mappings;
            };

            parametersPanel.Children.Add(mappingTextBox);

            // Set initial value
            var initialMappings = new Dictionary<string, string>
            {
                { "Male", "M" },
                { "Female", "F" },
                { "Unknown", "U" }
            };
            parameters["Mappings"] = initialMappings;
        }

        /// <summary>
        /// Updates the transformation preview with sample values
        /// </summary>
        private void UpdateTransformationPreview(StackPanel previewPanel, ColumnMappingViewModel mappingViewModel, TransformationType transformationType, Dictionary<string, object> parameters)
        {
            previewPanel.Children.Clear();

            try
            {
                var transformation = _transformationService.CreateTransformation(transformationType, mappingViewModel.SelectedCsvColumn);
                
                // Parameters are passed directly to Transform method, not stored in transformation

                // Get sample values (first 3)
                var sampleValues = mappingViewModel.SampleValues?.Take(3).ToList() ?? new List<string>();
                
                foreach (var sampleValue in sampleValues)
                {
                    var originalText = new TextBlock
                    {
                        Text = $"'{sampleValue}' → ",
                        Margin = new Thickness(0, 2, 0, 2),
                        FontFamily = new FontFamily("Consolas")
                    };

                    var transformedValue = transformation.Transform(sampleValue, parameters);
                    var transformedText = new TextBlock
                    {
                        Text = $"'{transformedValue}'",
                        Margin = new Thickness(0, 2, 0, 2),
                        FontFamily = new FontFamily("Consolas"),
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0))
                    };

                    var previewRow = new StackPanel { Orientation = Orientation.Horizontal };
                    previewRow.Children.Add(originalText);
                    previewRow.Children.Add(transformedText);
                    
                    previewPanel.Children.Add(previewRow);
                }

                if (!sampleValues.Any())
                {
                    var noDataText = new TextBlock
                    {
                        Text = "No sample data available for preview",
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128))
                    };
                    previewPanel.Children.Add(noDataText);
                }
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Preview error: {ex.Message}",
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                    FontStyle = FontStyles.Italic
                };
                previewPanel.Children.Add(errorText);
            }
        }



        /// <summary>
        /// Creates a UI element for a transformation option
        /// </summary>
        private UIElement CreateTransformationOption(TransformationType transformationType, ColumnMappingViewModel mappingViewModel)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            
            var button = new Button
            {
                Content = GetTransformationDisplayName(transformationType, mappingViewModel.DbColumn?.Name ?? ""),
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 5, 10, 5)
            };
            
            button.Click += (s, e) => ApplyTransformation(mappingViewModel, transformationType);
            
            panel.Children.Add(button);
            return panel;
        }

        /// <summary>
        /// Applies a transformation to a column mapping
        /// </summary>
        private void ApplyTransformation(ColumnMappingViewModel mappingViewModel, TransformationType transformationType)
        {
            try
            {
                var transformation = _transformationService.CreateTransformation(transformationType, mappingViewModel.SelectedCsvColumn);
                mappingViewModel.ApplyTransformation(transformation);
                
                // Update validation
                var mainViewModel = (MainViewModel)DataContext;
                if (mainViewModel?.CurrentMappingType != null)
                {
                    mainViewModel.ValidateMappings(mainViewModel.CurrentMappingType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying transformation: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string GetTransformationDisplayName(TransformationType type, string columnName)
        {
            switch (type)
            {
                case TransformationType.SplitFirstToken:
                    return "Extract First Word";
                case TransformationType.SplitLastToken:
                    return "Extract Last Word";
                case TransformationType.DateFormat:
                    return columnName.Contains("Year") ? "Extract Year from Date" : "Format Date";
                case TransformationType.CategoryMapping:
                    return columnName.Contains("Gender") ? "Standardize Gender Values" : "Map Categories";
                default:
                    return type.ToString();
            }
        }
        
        /// <summary>
        /// Clears the transformation for the selected column mapping
        /// </summary>
        private void ClearTransformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ColumnMappingViewModel mappingViewModel)
            {
                // Execute the clear transformation command
                mappingViewModel.ClearTransformationCommand.Execute(null);
                
                // Update validation
                var mainViewModel = (MainViewModel)DataContext;
                if (mainViewModel.CurrentMappingType != null)
                {
                    mainViewModel.ValidateMappings(mainViewModel.CurrentMappingType);
                }
            }
        }
    }
}