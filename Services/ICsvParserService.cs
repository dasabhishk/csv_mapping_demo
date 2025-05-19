using CsvMapper.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service interface for parsing CSV files
    /// </summary>
    public interface ICsvParserService
    {
        /// <summary>
        /// Parses a CSV file and extracts column headers and sample data
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>List of CSV columns with sample data and inferred types</returns>
        Task<List<CsvColumn>> ParseCsvFileAsync(string filePath);
    }
}
