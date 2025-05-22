using CsvMapper.Helpers;
using CsvMapper.Models;
using CsvMapper.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service for handling mappings between CSV and database columns
    /// </summary>
    public class MappingService : IMappingService
    {
        /// <summary>
        /// Validates mappings between CSV and database columns
        /// </summary>
        /// <param name="mappings">The list of column mappings to validate</param>
        /// <param name="csvColumns">Available CSV columns</param>
        /// <param name="dbColumns">Database columns that need mapping</param>
        /// <returns>A dictionary of validation errors by column name</returns>
        public Dictionary<string, string> ValidateMappings(
            List<ColumnMappingViewModel> mappings, 
            List<CsvColumn> csvColumns, 
            List<DatabaseColumn> dbColumns)
        {
            var errors = new Dictionary<string, string>();

            // Check if any required DB columns are not mapped
            foreach (var mapping in mappings)
            {
                if (string.IsNullOrEmpty(mapping.SelectedCsvColumn))
                {
                    errors[mapping.DbColumn.Name] = "This database column must be mapped to a CSV column.";
                    continue;
                }

                // Find the corresponding CSV column
                var csvColumn = csvColumns.FirstOrDefault(c => c.Name == mapping.SelectedCsvColumn);
                if (csvColumn == null)
                {
                    errors[mapping.DbColumn.Name] = $"Selected CSV column '{mapping.SelectedCsvColumn}' not found.";
                    continue;
                }

                // Check data type compatibility
                if (!IsTypeCompatible(csvColumn.InferredType, mapping.DbColumn.DataType))
                {
                    errors[mapping.DbColumn.Name] = $"Type mismatch: CSV column is '{csvColumn.InferredType}', but DB column requires '{mapping.DbColumn.DataType}'.";
                }
            }

            // Check for duplicate mappings (multiple DB columns mapping to the same CSV column)
            var duplicateMappings = mappings
                .Where(m => !string.IsNullOrEmpty(m.SelectedCsvColumn))
                .GroupBy(m => m.SelectedCsvColumn)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList();

            foreach (var duplicate in duplicateMappings)
            {
                if (!errors.ContainsKey(duplicate.DbColumn.Name))
                {
                    errors[duplicate.DbColumn.Name] = $"Multiple database columns are mapped to the same CSV column '{duplicate.SelectedCsvColumn}'.";
                }
            }

            return errors;
        }

        /// <summary>
        /// Saves a single mapping result to a JSON file
        /// </summary>
        /// <param name="mappingResult">The mapping result to save</param>
        /// <param name="filePath">Path where to save the mapping file</param>
        /// <returns>True if saving was successful</returns>
        public async Task<bool> SaveMappingsAsync(MappingResult mappingResult, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(mappingResult, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves multiple mapping results to a JSON file
        /// </summary>
        /// <param name="multiMappingResult">The multiple mapping results to save</param>
        /// <param name="filePath">Path where to save the mapping file</param>
        /// <returns>True if saving was successful</returns>
        public async Task<bool> SaveMultiMappingsAsync(MultiMappingResult multiMappingResult, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(multiMappingResult, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a mapping result from a JSON file
        /// </summary>
        /// <param name="filePath">Path to the mapping JSON file</param>
        /// <returns>The loaded mapping result</returns>
        public async Task<MultiMappingResult> LoadMappingsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new MultiMappingResult();
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                
                // Try to deserialize as MultiMappingResult first (new format)
                MultiMappingResult? multiResult = null;
                try
                {
                    multiResult = JsonConvert.DeserializeObject<MultiMappingResult>(json);
                }
                catch
                {
                    // If failed, it might be the old format (single mapping)
                    var singleResult = JsonConvert.DeserializeObject<MappingResult>(json);
                    if (singleResult != null)
                    {
                        multiResult = new MultiMappingResult
                        {
                            Mappings = new List<MappingResult> { singleResult }
                        };
                    }
                }

                return multiResult ?? new MultiMappingResult();
            }
            catch (Exception)
            {
                return new MultiMappingResult();
            }
        }

        /// <summary>
        /// Attempts to auto-match CSV columns to database columns based on name similarity
        /// </summary>
        /// <param name="csvColumns">Available CSV columns</param>
        /// <param name="dbColumns">Database columns that need mapping</param>
        /// <returns>Dictionary of suggested matches (db column name -> csv column name)</returns>
        public Dictionary<string, string> AutoMatchColumns(List<CsvColumn> csvColumns, List<DatabaseColumn> dbColumns)
        {
            var matches = new Dictionary<string, string>();

            // For simplicity, we'll do basic string matching
            // In a real implementation, this could use RapidFuzz.Net as mentioned in the requirements
            foreach (var dbColumn in dbColumns)
            {
                string dbColumnNameLower = dbColumn.Name.ToLowerInvariant();
                
                // Find exact matches first
                var exactMatch = csvColumns.FirstOrDefault(c => 
                    string.Equals(c.Name, dbColumn.Name, StringComparison.OrdinalIgnoreCase));
                
                if (exactMatch != null)
                {
                    matches[dbColumn.Name] = exactMatch.Name;
                    continue;
                }

                // If no exact match, find similar names
                foreach (var csvColumn in csvColumns)
                {
                    string csvColumnNameLower = csvColumn.Name.ToLowerInvariant();
                    
                    // Check if CSV column name contains DB column name with no case sensitivity
                    if (csvColumnNameLower.Contains(dbColumnNameLower) || 
                        dbColumnNameLower.Contains(csvColumnNameLower))
                    {
                        matches[dbColumn.Name] = csvColumn.Name;
                        break;
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Checks if the CSV type is compatible with the database type
        /// </summary>
        private static bool IsTypeCompatible(string csvType, string dbType)
        {
            // Normalize types to lower case for comparison
            csvType = csvType.ToLowerInvariant();
            dbType = dbType.ToLowerInvariant();

            // Direct match
            if (csvType == dbType)
                return true;

            // Special compatibility rules
            return dbType switch
            {
                "string" => true, // Any type can be stored as string
                "int" => csvType == "int",
                "decimal" or "float" or "double" => csvType is "int" or "decimal" or "float" or "double",
                "datetime" => csvType == "datetime",
                "bool" or "boolean" => csvType is "bool" or "boolean",
                _ => csvType == dbType // For unknown types, require exact match
            };
        }
    }
}
