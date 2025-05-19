using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a column from a CSV file with its sample values and inferred data type
    /// </summary>
    public class CsvColumn
    {
        /// <summary>
        /// The name of the column as read from the CSV header
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sample values from the first few rows (up to 5)
        /// </summary>
        public List<string> SampleValues { get; set; } = new List<string>();

        /// <summary>
        /// The inferred data type based on the sample values
        /// </summary>
        public string InferredType { get; set; } = string.Empty;

        /// <summary>
        /// Index of this column in the CSV file
        /// </summary>
        public int Index { get; set; }
    }
}
