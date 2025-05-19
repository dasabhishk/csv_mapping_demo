using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents the final mapping result to be saved to mappings.json
    /// </summary>
    public class MappingResult
    {
        /// <summary>
        /// The name of the database table
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// The list of column mappings (CSV to DB)
        /// </summary>
        public List<ColumnMapping> ColumnMappings { get; set; } = new List<ColumnMapping>();
    }
}
