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
        /// The CSV type associated with this mapping (PatientStudy or SeriesInstance)
        /// </summary>
        public string CsvType { get; set; } = string.Empty;

        /// <summary>
        /// The list of column mappings (CSV to DB)
        /// </summary>
        public List<ColumnMapping> ColumnMappings { get; set; } = new List<ColumnMapping>();
    }

    /// <summary>
    /// Container for multiple mapping results for different CSV types
    /// </summary>
    public class MultiMappingResult
    {
        /// <summary>
        /// List of all mappings for different CSV types
        /// </summary>
        public List<MappingResult> Mappings { get; set; } = new List<MappingResult>();
    }
}
