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
        /// Saves mapping results to a JSON file
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
        /// Attempts to auto-match CSV columns to database columns based on name similarity
        /// </summary>
        /// <param name="csvColumns">Available CSV columns</param>
        /// <param name="dbColumns">Database columns that need mapping</param>
        /// <returns>Dictionary of suggested matches (db column name -> csv column name)</returns>
        public Dictionary<string, string> AutoMatchColumns(List<CsvColumn> csvColumns, List<DatabaseColumn> dbColumns)
        {
            var matches = new Dictionary<string, string>();

            // For simplicity, we'll do basic string matching
            // In a real implementation, we could use RapidFuzz.Net
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
        private bool IsTypeCompatible(string csvType, string dbType)
        {
            // Normalize types to lower case for comparison
            csvType = csvType.ToLowerInvariant();
            dbType = dbType.ToLowerInvariant();

            // Direct match
            if (csvType == dbType)
                return true;

            // Special compatibility rules
            switch (dbType)
            {
                case "string":
                    // Any type can be stored as string
                    return true;
                case "int":
                    return csvType == "int";
                case "decimal":
                case "float":
                case "double":
                    return csvType == "int" || csvType == "decimal" || csvType == "float" || csvType == "double";
                case "datetime":
                    return csvType == "datetime";
                case "bool":
                case "boolean":
                    return csvType == "bool" || csvType == "boolean";
                default:
                    // For unknown types, require exact match
                    return csvType == dbType;
            }
        }
    }
}
