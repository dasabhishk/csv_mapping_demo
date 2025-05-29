using System;

namespace CsvMapper.Models.Transformations
{
    /// <summary>
    /// Defines the types of transformations available for derived columns
    /// </summary>
    public enum TransformationType
    {
        // Text transformations
        SplitFirstToken,      // Get first token from text using delimiter
        SplitLastToken,       // Get last token from text using delimiter
        RegexExtract,         // Extract text using regex pattern
        
        // Date transformations
        DateFormat,           // Format a date string to a specific format
        DateExtractComponent, // Extract year, month, day, etc. from a date
        
        // Category transformations
        CategoryMapping,      // Map values to standardized categories (e.g., gender codes)
        
        // Number transformations
        NumberFormat,         // Format a number (decimal places, thousands separator, etc.)
        UnitConversion        // Convert between units (e.g., inches to cm)
    }
}