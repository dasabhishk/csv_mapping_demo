using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a database schema containing multiple tables
    /// </summary>
    public class DatabaseSchema
    {
        /// <summary>
        /// Name of the database
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Tables in the database
        /// </summary>
        public List<TableSchema> Tables { get; set; } = new List<TableSchema>();
    }
}