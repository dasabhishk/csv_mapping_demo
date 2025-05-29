using System.Collections.Generic;

namespace CsvMapper.Models
{
    /// <summary>
    /// Represents a database table schema - identical to SchemaTable
    /// for compatibility with existing code
    /// </summary>
    public class TableSchema
    {
        /// <summary>
        /// Name of the table
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Type of CSV file that maps to this table
        /// </summary>
        public string CsvType { get; set; } = string.Empty;

        /// <summary>
        /// Columns in this table
        /// </summary>
        public List<DatabaseColumn> Columns { get; set; } = new List<DatabaseColumn>();
        
        /// <summary>
        /// Implicit conversion from TableSchema to SchemaTable
        /// </summary>
        public static implicit operator SchemaTable(TableSchema tableSchema)
        {
            return new SchemaTable
            {
                TableName = tableSchema.TableName,
                CsvType = tableSchema.CsvType,
                Columns = tableSchema.Columns
            };
        }
    }
}