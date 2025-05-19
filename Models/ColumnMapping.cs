namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a mapping between a CSV column and a database column
    /// </summary>
    public class ColumnMapping
    {
        /// <summary>
        /// Name of the CSV column
        /// </summary>
        public string CsvColumn { get; set; } = string.Empty;

        /// <summary>
        /// Name of the database column
        /// </summary>
        public string DbColumn { get; set; } = string.Empty;
    }
}
