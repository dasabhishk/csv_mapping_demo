namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a column in the database schema
    /// </summary>
    public class DatabaseColumn
    {
        /// <summary>
        /// Name of the database column
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data type of the database column
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the column is required (not nullable)
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// Maximum length for string columns
        /// </summary>
        public int? MaxLength { get; set; }
        
        /// <summary>
        /// Whether this column allows transformations
        /// </summary>
        public bool CanTransform { get; set; }
    }
}
