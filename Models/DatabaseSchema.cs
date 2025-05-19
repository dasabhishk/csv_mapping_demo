using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents the complete database schema
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
        public List<SchemaTable> Tables { get; set; } = new List<SchemaTable>();
    }
}
