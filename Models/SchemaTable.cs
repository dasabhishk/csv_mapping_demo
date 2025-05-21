using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a table in the database schema
    /// </summary>
    public class SchemaTable
    {
        /// <summary>
        /// Name of the table
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// CSV type that maps to this table (PatientStudy or SeriesInstance)
        /// </summary>
        public string CsvType { get; set; } = string.Empty;

        /// <summary>
        /// Columns in the table
        /// </summary>
        public List<DatabaseColumn> Columns { get; set; } = new List<DatabaseColumn>();
    }
}
