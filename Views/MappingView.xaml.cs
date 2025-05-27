using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using CsvMapper.Services;
using CsvMapper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeCommand.ExecuteAsync(null);
            }
        }
        
        /// <summary>
        /// Opens the transformation dialog for the selected column mapping
        /// </summary>
        private void TransformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ColumnMappingViewModel mappingViewModel)
            {
                // Make sure a CSV column is selected
                if (string.IsNullOrEmpty(mappingViewModel.SelectedCsvColumn))
                {
                    MessageBox.Show("Please select a CSV column to transform.", "Missing Column Selection", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Get current mapping type from MainViewModel
                var mainViewModel = (MainViewModel)DataContext;
                
                // Find the source column
                var sourceColumn = mainViewModel.CurrentMappingType?.CsvColumns.Find(c => c.Name == mappingViewModel.SelectedCsvColumn);
                if (mainViewModel.CurrentMappingType == null)
                {
                    MessageBox.Show("No mapping type is currently selected.", "No Mapping Type", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (sourceColumn == null)
                {
                    MessageBox.Show($"Could not find source column: {mappingViewModel.SelectedCsvColumn}", 
                        "Column Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                try
                {
                    // Create temporary dialog with transformation types
                    var dialog = new Window()
                    {
                        Title = "Apply Column Transformation",
                        Width = 500,
                        Height = 400,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = Window.GetWindow(this)
                    };
                    
                    // Build dialog content
                    var panel = new StackPanel() { Margin = new Thickness(15) };
                    
                    // Header & Column Info
                    panel.Children.Add(new TextBlock() 
                    { 
                        Text = "Apply Transformation", 
                        FontSize = 16, 
                        FontWeight = FontWeights.Bold, 
                        Margin = new Thickness(0, 0, 0, 10) 
                    });
                    
                    var infoGrid = new Grid() { Margin = new Thickness(0, 0, 0, 10) };
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    
                    var srcLabel = new TextBlock() { Text = "Source:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 5, 0) };
                    var srcValue = new TextBlock() { Text = mappingViewModel.SelectedCsvColumn };
                    var targetLabel = new TextBlock() { Text = "Target:", FontWeight = FontWeights.Bold, Margin = new Thickness(10, 0, 5, 0) };
                    var targetValue = new TextBlock() { Text = mappingViewModel.DbColumn.Name };
                    
                    Grid.SetColumn(srcLabel, 0);
                    Grid.SetColumn(srcValue, 1);
                    Grid.SetColumn(targetLabel, 2);
                    Grid.SetColumn(targetValue, 3);
                    
                    infoGrid.Children.Add(srcLabel);
                    infoGrid.Children.Add(srcValue);
                    infoGrid.Children.Add(targetLabel);
                    infoGrid.Children.Add(targetValue);
                    
                    panel.Children.Add(infoGrid);
                    
                    // Make sure transformations are up-to-date
                    mappingViewModel.UpdateAvailableTransformations();
                    
                    // Create transformation selector
                    var transformGroup = new GroupBox() { Header = "Transformation Type", Margin = new Thickness(0, 0, 0, 10) };
                    var transformCombo = new ComboBox() { Margin = new Thickness(5) };
                    
                    // Add available transformations to combo
                    foreach (var type in mappingViewModel.AvailableTransformations)
                    {
                        string displayName = GetTransformationDisplayName(type, mappingViewModel.DbColumn.Name);
                        transformCombo.Items.Add(new ComboBoxItem() 
                        { 
                            Content = displayName, 
                            Tag = type 
                        });
                    }
                    
                    if (transformCombo.Items.Count > 0)
                        transformCombo.SelectedIndex = 0;
                    
                    transformGroup.Content = transformCombo;
                    panel.Children.Add(transformGroup);
                    
                    // Configuration section
                    var configGroup = new GroupBox() { Header = "Configuration", Margin = new Thickness(0, 0, 0, 10) };
                    var configStack = new StackPanel() { Margin = new Thickness(5) };
                    
                    // Configuration controls for different transformation types
                    var splitConfig = new StackPanel() { Margin = new Thickness(5), Visibility = Visibility.Collapsed };
                    splitConfig.Children.Add(new TextBlock() { Text = "Delimiter:" });
                    var delimiterTextBox = new TextBox() { Text = " ", Width = 150, HorizontalAlignment = HorizontalAlignment.Left };
                    splitConfig.Children.Add(delimiterTextBox);
                    
                    var dateConfig = new StackPanel() { Margin = new Thickness(5), Visibility = Visibility.Collapsed };
                    dateConfig.Children.Add(new TextBlock() { Text = "Format:" });
                    var formatCombo = new ComboBox() { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                    formatCombo.Items.Add(new ComboBoxItem() { Content = "ISO Date (yyyy-MM-dd)", Tag = "yyyy-MM-dd" });
                    formatCombo.Items.Add(new ComboBoxItem() { Content = "US Date (MM/dd/yyyy)", Tag = "MM/dd/yyyy" });
                    formatCombo.Items.Add(new ComboBoxItem() { Content = "European (dd/MM/yyyy)", Tag = "dd/MM/yyyy" });
                    formatCombo.Items.Add(new ComboBoxItem() { Content = "Year Only (yyyy)", Tag = "yyyy" });
                    if (formatCombo.Items.Count > 0)
                        formatCombo.SelectedIndex = 0;
                    dateConfig.Children.Add(formatCombo);
                    
                    var categoryConfig = new StackPanel() { Margin = new Thickness(5), Visibility = Visibility.Collapsed };
                    categoryConfig.Children.Add(new TextBlock() { Text = "Default Value:" });
                    var defaultValueTextBox = new TextBox() { Width = 150, HorizontalAlignment = HorizontalAlignment.Left };
                    categoryConfig.Children.Add(defaultValueTextBox);
                    
                    configStack.Children.Add(splitConfig);
                    configStack.Children.Add(dateConfig);
                    configStack.Children.Add(categoryConfig);
                    
                    configGroup.Content = configStack;
                    panel.Children.Add(configGroup);
                    
                    // Update configuration panel when transformation type changes
                    transformCombo.SelectionChanged += (sender, e) => 
                    {
                        // Hide all configuration panels
                        splitConfig.Visibility = Visibility.Collapsed;
                        dateConfig.Visibility = Visibility.Collapsed;
                        categoryConfig.Visibility = Visibility.Collapsed;
                        
                        // Show appropriate panel based on selection
                        if (transformCombo.SelectedItem is ComboBoxItem selected && selected.Tag is Models.Transformations.TransformationType type)
                        {
                            switch (type)
                            {
                                case Models.Transformations.TransformationType.SplitFirstToken:
                                case Models.Transformations.TransformationType.SplitLastToken:
                                    splitConfig.Visibility = Visibility.Visible;
                                    break;
                                case Models.Transformations.TransformationType.DateFormat:
                                    dateConfig.Visibility = Visibility.Visible;
                                    break;
                                case Models.Transformations.TransformationType.CategoryMapping:
                                    categoryConfig.Visibility = Visibility.Visible;
                                    break;
                            }
                        }
                    };
                    
                    // Initial selection to show correct panel
                    if (transformCombo.SelectedItem is ComboBoxItem initial && initial.Tag is Models.Transformations.TransformationType initialType)
                    {
                        switch (initialType)
                        {
                            case Models.Transformations.TransformationType.SplitFirstToken:
                            case Models.Transformations.TransformationType.SplitLastToken:
                                splitConfig.Visibility = Visibility.Visible;
                                break;
                            case Models.Transformations.TransformationType.DateFormat:
                                dateConfig.Visibility = Visibility.Visible;
                                break;
                            case Models.Transformations.TransformationType.CategoryMapping:
                                categoryConfig.Visibility = Visibility.Visible;
                                break;
                        }
                    }
                    
                    // Buttons
                    var buttonPanel = new StackPanel() 
                    { 
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    
                    var applyButton = new Button() { Content = "Apply", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
                    var cancelButton = new Button() { Content = "Cancel", Width = 75 };
                    
                    buttonPanel.Children.Add(applyButton);
                    buttonPanel.Children.Add(cancelButton);
                    
                    panel.Children.Add(buttonPanel);
                    
                    // Add panel to dialog
                    dialog.Content = panel;
                    
                    // Button handlers
                    bool dialogResult = false;
                    
                    applyButton.Click += (s, args) => 
                    {
                        try
                        {
                            // Get selected transformation type
                            if (transformCombo.SelectedItem is ComboBoxItem selectedItem && 
                                selectedItem.Tag is Models.Transformations.TransformationType selectedType)
                            {
                                // Get transformation parameters
                                var parameters = new Dictionary<string, object>();
                                
                                switch (selectedType)
                                {
                                    case Models.Transformations.TransformationType.SplitFirstToken:
                                    case Models.Transformations.TransformationType.SplitLastToken:
                                        parameters["Delimiter"] = delimiterTextBox.Text;
                                        break;
                                        
                                    case Models.Transformations.TransformationType.DateFormat:
                                        string format = "yyyy-MM-dd"; // Default
                                        if (formatCombo.SelectedItem is ComboBoxItem formatItem)
                                            format = formatItem.Tag?.ToString() ?? format;
                                        parameters["TargetFormat"] = format;
                                        break;
                                        
                                    case Models.Transformations.TransformationType.CategoryMapping:
                                        parameters["DefaultValue"] = defaultValueTextBox.Text ?? "";
                                        parameters["CaseSensitive"] = false;
                                        
                                        // For simplicity, if this is gender mapping, add standard mappings
                                        if (mappingViewModel.DbColumn.Name.Contains("Gender"))
                                        {
                                            var mappings = new Dictionary<string, string>
                                            {
                                                { "Male", "M" },
                                                { "Female", "F" },
                                                { "M", "M" },
                                                { "F", "F" },
                                                { "male", "M" },
                                                { "female", "F" }
                                            };
                                            parameters["Mappings"] = mappings;
                                        }
                                        else
                                        {
                                            parameters["Mappings"] = new Dictionary<string, string>();
                                        }
                                        break;
                                }
                                
                                // Apply transformation to sample values
                                var originalValues = new List<string>(sourceColumn.SampleValues);
                                var transformedValues = _transformationService.TransformSamples(
                                    originalValues,
                                    selectedType,
                                    parameters);
                                
                                // Update mapping view model
                                mappingViewModel.HasTransformation = true;
                                mappingViewModel.TransformationType = selectedType;
                                mappingViewModel.TransformationParameters = parameters;
                                mappingViewModel.TransformationDisplayName = selectedItem.Content?.ToString() ?? "Transformation";
                                mappingViewModel.OriginalSampleValues = new ObservableCollection<string>(originalValues);
                                mappingViewModel.SampleValues = new ObservableCollection<string>(transformedValues);
                                
                                dialogResult = true;
                                dialog.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error applying transformation: {ex.Message}", 
                                "Transformation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    
                    cancelButton.Click += (s, args) => 
                    {
                        dialogResult = false;
                        dialog.Close();
                    };
                    
                    // Show dialog
                    dialog.ShowDialog();
                    
                    // Process result
                    if (dialogResult)
                    {
                        // Transformation was applied, update validation
                        mainViewModel.ValidateMappings(mainViewModel.CurrentMappingType);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error applying transformation: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private string GetTransformationDisplayName(Models.Transformations.TransformationType type, string columnName)
        {
            switch (type)
            {
                case Models.Transformations.TransformationType.SplitFirstToken:
                    return "Extract First Word";
                case Models.Transformations.TransformationType.SplitLastToken:
                    return "Extract Last Word";
                case Models.Transformations.TransformationType.DateFormat:
                    return columnName.Contains("Year") ? "Extract Year from Date" : "Format Date";
                case Models.Transformations.TransformationType.CategoryMapping:
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
