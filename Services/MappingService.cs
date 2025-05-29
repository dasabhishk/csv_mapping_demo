using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using CsvMapper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service for managing CSV to database mappings
    /// </summary>
    public class MappingService : IMappingService
    {
        private readonly ITransformationService _transformationService;

        /// <summary>
        /// Constructor with injected dependencies
        /// </summary>
        public MappingService(ITransformationService transformationService)
        {
            _transformationService = transformationService;
        }

        /// <summary>
        /// Attempts to automatically match CSV columns to database columns
        /// based on name similarity
        /// </summary>
        public Dictionary<string, string> AutoMatchColumns(ObservableCollection<CsvColumn> csvColumns, List<DatabaseColumn> dbColumns)
        {
            var result = new Dictionary<string, string>();
            
            foreach (var dbColumn in dbColumns)
            {
                // Try to find an exact match first
                var exactMatch = csvColumns.FirstOrDefault(c => 
                    string.Equals(c.Name, dbColumn.Name, StringComparison.OrdinalIgnoreCase));
                
                if (exactMatch != null)
                {
                    result[dbColumn.Name] = exactMatch.Name;
                    continue;
                }
                
                // Try to find a match by removing spaces and underscores
                string normalizedDbName = NormalizeColumnName(dbColumn.Name);
                var normalizedMatch = csvColumns.FirstOrDefault(c => 
                    string.Equals(NormalizeColumnName(c.Name), normalizedDbName, StringComparison.OrdinalIgnoreCase));
                
                if (normalizedMatch != null)
                {
                    result[dbColumn.Name] = normalizedMatch.Name;
                    continue;
                }
                
                // Try to match based on contained text (e.g., "FirstName" matches "PatientFirstName")
                var containsMatch = csvColumns.FirstOrDefault(c => 
                    c.Name.Contains(dbColumn.Name, StringComparison.OrdinalIgnoreCase) || 
                    dbColumn.Name.Contains(c.Name, StringComparison.OrdinalIgnoreCase));
                
                if (containsMatch != null)
                {
                    result[dbColumn.Name] = containsMatch.Name;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Validates a mapping between CSV columns and database columns
        /// Returns hard validation errors only - warnings are handled separately
        /// </summary>
        public Dictionary<string, string> ValidateMappings(
            List<ViewModels.ColumnMappingViewModel> mappingViewModels, 
            ObservableCollection<CsvColumn> csvColumns, 
            List<DatabaseColumn> dbColumns)
        {
            var errors = new Dictionary<string, string>();
            var warnings = new Dictionary<string, string>();
            
            // Check for duplicate CSV column mappings (especially name fields)
            var csvColumnUsage = mappingViewModels
                .Where(vm => !string.IsNullOrEmpty(vm.SelectedCsvColumn) && vm.SelectedCsvColumn != "-- No Mapping (Optional) --")
                .GroupBy(vm => vm.SelectedCsvColumn)
                .Where(g => g.Count() > 1)
                .ToList();
            
            foreach (var duplicateGroup in csvColumnUsage)
            {
                string csvColumnName = duplicateGroup.Key;
                var mappedDbColumns = duplicateGroup.Select(vm => vm.DbColumn.Name).ToList();
                
                // Special handling for name and date/DOB fields - show warning but allow saving
                if (csvColumnName.ToLower().Contains("name") || 
                    csvColumnName.ToLower().Contains("date") || 
                    csvColumnName.ToLower().Contains("dob") ||
                    csvColumnName.ToLower().Contains("birth"))
                {
                    foreach (var vm in duplicateGroup)
                    {
                        warnings[vm.DbColumn.Name] = $"WARNING: CSV column '{csvColumnName}' is mapped to multiple fields ({string.Join(", ", mappedDbColumns)}). Consider using transformations to split this field appropriately.";
                    }
                }
                else
                {
                    // For non-name/date fields, this is a hard error
                    foreach (var vm in duplicateGroup)
                    {
                        errors[vm.DbColumn.Name] = $"ERROR: CSV column '{csvColumnName}' cannot be mapped to multiple database columns ({string.Join(", ", mappedDbColumns)})";
                    }
                }
            }
            
            foreach (var dbColumn in dbColumns)
            {
                // Get the mapping view model for this DB column
                var mappingVm = mappingViewModels.FirstOrDefault(vm => vm.DbColumn.Name == dbColumn.Name);
                if (mappingVm == null)
                {
                    continue; // No mapping for this column
                }
                
                // Check if required columns are mapped
                if (dbColumn.IsRequired && (string.IsNullOrEmpty(mappingVm.SelectedCsvColumn) || mappingVm.SelectedCsvColumn == "-- No Mapping (Optional) --"))
                {
                    errors[dbColumn.Name] = "This column is required but not mapped";
                    continue;
                }
                
                // Skip validation for unmapped columns
                if (string.IsNullOrEmpty(mappingVm.SelectedCsvColumn) || mappingVm.SelectedCsvColumn == "-- No Mapping (Optional) --")
                {
                    continue;
                }
                
                // Get the CSV column
                var csvColumn = csvColumns.FirstOrDefault(c => c.Name == mappingVm.SelectedCsvColumn);
                if (csvColumn == null)
                {
                    errors[dbColumn.Name] = $"Mapped CSV column '{mappingVm.SelectedCsvColumn}' not found";
                    continue;
                }
                
                // Get the samples to validate - use transformed values if available
                List<string> samplesToValidate;
                if (mappingVm.HasTransformation)
                {
                    samplesToValidate = mappingVm.SampleValues.ToList();
                }
                else
                {
                    samplesToValidate = csvColumn.SampleValues;
                }

                // For transformed values, we don't rely on the CSV column's inferred type
                // Instead, we validate the actual transformed values against the DB column type
                string effectiveSourceType = mappingVm.HasTransformation 
                    ? InferTypeFromSamples(samplesToValidate) 
                    : csvColumn.InferredType;
                
                // For transformations, if the types aren't compatible but the transformation 
                // was explicitly chosen by the user, we trust that the transformation will 
                // handle the conversion properly
                if (mappingVm.HasTransformation)
                {
                    // Skip type validation for transformed values - we assume the transformation
                    // was chosen to fix type compatibility issues
                    // Just perform basic validation to ensure it's not completely invalid
                    bool hasNonEmptyValues = samplesToValidate.Any(s => !string.IsNullOrWhiteSpace(s));
                    if (!hasNonEmptyValues && dbColumn.IsRequired)
                    {
                        errors[dbColumn.Name] = "Required column has no valid values after transformation";
                        continue;
                    }
                }
                else 
                {
                    // For non-transformed columns, validate data type compatibility normally
                    if (!IsTypeCompatible(effectiveSourceType, dbColumn.DataType))
                    {
                        errors[dbColumn.Name] = $"Data type mismatch: CSV value is '{effectiveSourceType}', DB is '{dbColumn.DataType}'";
                        continue;
                    }
                }
                
                // Validate string length for string types
                if (dbColumn.DataType == "string")
                {
                    // We'll check the maximum allowed length (default to 4000 if not specified)
                    int maxLength = dbColumn.MaxLength ?? 4000;
                    
                    bool hasLongValue = samplesToValidate.Any(v => 
                        !string.IsNullOrEmpty(v) && v.Length > maxLength);
                    
                    if (hasLongValue)
                    {
                        errors[dbColumn.Name] = $"Some values exceed the maximum length of {maxLength}";
                        continue;
                    }
                    
                    // Additional validation for specific field types when transformed
                    if (mappingVm.HasTransformation)
                    {
                        // Validate physician name format (if applicable)
                        if (dbColumn.Name.Contains("Physician"))
                        {
                            bool validFormat = ValidatePhysicianNameFormat(samplesToValidate);
                            if (!validFormat)
                            {
                                errors[dbColumn.Name] = "Transformed physician names should follow 'LastName, FirstName Title' format";
                                continue;
                            }
                        }
                    }
                }
            }
            
            // Store warnings in the ViewModels for UI display
            foreach (var warning in warnings)
            {
                var mappingVm = mappingViewModels.FirstOrDefault(vm => vm.DbColumn.Name == warning.Key);
                if (mappingVm != null)
                {
                    mappingVm.ValidationWarning = warning.Value;
                }
            }
            
            return errors;
        }

        /// <summary>
        /// Loads mappings from a JSON file
        /// </summary>
        public async Task<MultiMappingResult> LoadMappingsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new MultiMappingResult();
            }
            
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<MultiMappingResult>(json, options) ?? new MultiMappingResult();
            }
            catch (Exception)
            {
                // If there's an error loading mappings, return an empty result
                return new MultiMappingResult();
            }
        }

        /// <summary>
        /// Saves mappings to a JSON file
        /// </summary>
        public async Task<bool> SaveMultiMappingsAsync(MultiMappingResult mappings, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(mappings, options);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates transformations for derived columns
        /// </summary>
        public DerivedColumn CreateDerivedColumn(
            CsvColumn sourceColumn,
            string newColumnName,
            TransformationType transformationType,
            Dictionary<string, object> parameters)
        {
            return _transformationService.CreateDerivedColumn(
                sourceColumn, newColumnName, transformationType, parameters);
        }

        /// <summary>
        /// Creates a derived column based on a saved mapping
        /// </summary>
        public DerivedColumn? RecreateTransformedColumn(
            CsvColumn sourceColumn,
            string derivedColumnName,
            string transformationType,
            string transformationParametersJson)
        {
            try
            {
                // Parse the transformation type
                if (!Enum.TryParse<TransformationType>(
                    transformationType, out var type))
                {
                    return null;
                }
                
                // Parse the parameters
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    transformationParametersJson, options) ?? new Dictionary<string, object>();
                
                // Create the derived column
                return _transformationService.CreateDerivedColumn(
                    sourceColumn, derivedColumnName, type, parameters);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Normalizes a column name for comparison by removing spaces, underscores, etc.
        /// </summary>
        private string NormalizeColumnName(string name)
        {
            return name.Replace(" ", "").Replace("_", "").Replace("-", "");
        }
        
        /// <summary>
        /// Infers the data type from a collection of sample values
        /// </summary>
        private string InferTypeFromSamples(IEnumerable<string> samples)
        {
            if (samples == null || !samples.Any())
                return "string"; // Default to string for empty collections
            
            bool allEmpty = samples.All(string.IsNullOrWhiteSpace);
            if (allEmpty)
                return "string";
                
            // Check if all values are valid integers
            bool allInts = samples
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .All(s => int.TryParse(s, out _));
                
            if (allInts)
                return "int";
                
            // Check if all values are valid decimals
            bool allDecimals = samples
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .All(s => decimal.TryParse(s, out _));
                
            if (allDecimals)
                return "decimal";
                
            // Check if all values are valid dates
            bool allDates = samples
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .All(s => DateTime.TryParse(s, out _));
                
            if (allDates)
                return "date";
                
            // If we can't determine a more specific type, default to string
            return "string";
        }
        
        /// <summary>
        /// Validates that physician names follow the expected format
        /// </summary>
        private bool ValidatePhysicianNameFormat(IEnumerable<string> physicianNames)
        {
            if (physicianNames == null || !physicianNames.Any())
                return true; // Empty values are considered valid
                
            // For physician names, we expect "LastName, FirstName Title" format
            // At minimum, we require "LastName, FirstName"
            foreach (var name in physicianNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue; // Skip empty names
                    
                // Check if name has at least a comma separating last and first name
                if (!name.Contains(","))
                    return false;
                    
                // Check that there's text before and after the comma
                var parts = name.Split(',', 2);
                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if the inferred CSV type is compatible with the DB column type
        /// </summary>
        private bool IsTypeCompatible(string csvType, string dbType)
        {
            // If either type is string, they're compatible (through conversion)
            if (csvType == "string" || dbType == "string")
                return true;
            
            // Same types are always compatible
            if (csvType == dbType)
                return true;
            
            // Specific type compatibility checks
            switch (dbType)
            {
                case "int":
                    return csvType == "int" || csvType == "decimal";
                
                case "decimal":
                case "double":
                case "float":
                    return csvType == "int" || csvType == "decimal" || 
                           csvType == "double" || csvType == "float";
                
                case "datetime":
                    return csvType == "datetime" || csvType == "string";
                
                case "bool":
                    return csvType == "bool" || csvType == "int";
            }
            
            // Default to incompatible
            return false;
        }
    }
}