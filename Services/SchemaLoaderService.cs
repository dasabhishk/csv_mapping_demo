using CsvMapper.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service for loading database schema from JSON file
    /// </summary>
    public class SchemaLoaderService : ISchemaLoaderService
    {
        /// <summary>
        /// Loads database schema from a JSON file
        /// </summary>
        /// <param name="filePath">Path to the schema JSON file</param>
        /// <returns>The database schema with tables and columns</returns>
        public async Task<DatabaseSchema> LoadSchemaAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified schema file was not found.", filePath);
            }

            try
            {
                string jsonContent = await File.ReadAllTextAsync(filePath);
                var schema = JsonConvert.DeserializeObject<DatabaseSchema>(jsonContent)
                             ?? throw new InvalidOperationException("Failed to deserialize the schema file to a valid DatabaseSchema object.");

                return schema;
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
            {
                throw new Exception($"Error loading schema file: {ex.Message}", ex);
            }
        }
    }
}
