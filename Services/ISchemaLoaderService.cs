using CsvMapper.Models;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service interface for loading database schema
    /// </summary>
    public interface ISchemaLoaderService
    {
        /// <summary>
        /// Loads database schema from a JSON file
        /// </summary>
        /// <param name="filePath">Path to the schema JSON file</param>
        /// <returns>The database schema with tables and columns</returns>
        Task<DatabaseSchema> LoadSchemaAsync(string filePath);
    }
}
