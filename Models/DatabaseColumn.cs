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
    }
}
