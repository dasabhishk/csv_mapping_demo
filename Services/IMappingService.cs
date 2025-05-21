using CsvMapper.Models;
using CsvMapper.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service interface for column mapping operations
    /// </summary>
    public interface IMappingService
    {
        /// <summary>
        /// Validates mappings between CSV and database columns
        /// </summary>
        /// <param name="mappings">The list of column mappings to validate</param>
        /// <param name="csvColumns">Available CSV columns</param>
        /// <param name="dbColumns">Database columns that need mapping</param>
        /// <returns>A dictionary of validation errors by column name</returns>
        Dictionary<string, string> ValidateMappings(
            List<ColumnMappingViewModel> mappings, 
            List<CsvColumn> csvColumns, 
            List<DatabaseColumn> dbColumns);

        /// <summary>
        /// Saves a single mapping result to a JSON file
        /// </summary>
        /// <param name="mappingResult">The mapping result to save</param>
        /// <param name="filePath">Path where to save the mapping file</param>
        /// <returns>True if saving was successful</returns>
        Task<bool> SaveMappingsAsync(MappingResult mappingResult, string filePath);

        /// <summary>
        /// Saves multiple mapping results to a JSON file
        /// </summary>
        /// <param name="multiMappingResult">The multiple mapping results to save</param>
        /// <param name="filePath">Path where to save the mapping file</param>
        /// <returns>True if saving was successful</returns>
        Task<bool> SaveMultiMappingsAsync(MultiMappingResult multiMappingResult, string filePath);

        /// <summary>
        /// Loads a mapping result from a JSON file
        /// </summary>
        /// <param name="filePath">Path to the mapping JSON file</param>
        /// <returns>The loaded mapping result</returns>
        Task<MultiMappingResult> LoadMappingsAsync(string filePath);

        /// <summary>
        /// Attempts to auto-match CSV columns to database columns based on name similarity
        /// </summary>
        /// <param name="csvColumns">Available CSV columns</param>
        /// <param name="dbColumns">Database columns that need mapping</param>
        /// <returns>Dictionary of suggested matches (db column name -> csv column name)</returns>
        Dictionary<string, string> AutoMatchColumns(List<CsvColumn> csvColumns, List<DatabaseColumn> dbColumns);
    }
}
