using System.Text.Json.Serialization;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a mapping between a CSV column and a database column
    /// </summary>
    public class ColumnMapping
    {
        /// <summary>
        /// The CSV column name (original or derived)
        /// </summary>
        public string CsvColumn { get; set; } = string.Empty;
        
        /// <summary>
        /// The database column name
        /// </summary>
        public string DbColumn { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates if this mapping uses a derived column
        /// </summary>
        public bool IsDerivedColumn { get; set; }
        
        /// <summary>
        /// For derived columns, this is the name of the source column
        /// </summary>
        public string? SourceColumnName { get; set; }
        
        /// <summary>
        /// For derived columns, this is the type of transformation
        /// </summary>
        public string? TransformationType { get; set; }
        
        /// <summary>
        /// For derived columns, this is a JSON string of transformation parameters
        /// </summary>
        public string? TransformationParameters { get; set; }
        
        /// <summary>
        /// Creates a simple mapping between a CSV column and database column
        /// </summary>
        public static ColumnMapping Create(string csvColumnName, string dbColumnName)
        {
            return new ColumnMapping
            {
                CsvColumn = csvColumnName,
                DbColumn = dbColumnName,
                IsDerivedColumn = false,
                SourceColumnName = null,
                TransformationType = null,
                TransformationParameters = null
            };
        }
        
        /// <summary>
        /// Creates a mapping for a derived column
        /// </summary>
        public static ColumnMapping CreateForDerivedColumn(
            string derivedColumnName, 
            string dbColumnName, 
            string sourceColumnName, 
            string transformationType, 
            string transformationParameters)
        {
            return new ColumnMapping
            {
                CsvColumn = derivedColumnName,
                DbColumn = dbColumnName,
                IsDerivedColumn = true,
                SourceColumnName = sourceColumnName,
                TransformationType = transformationType,
                TransformationParameters = transformationParameters
            };
        }
    }
}