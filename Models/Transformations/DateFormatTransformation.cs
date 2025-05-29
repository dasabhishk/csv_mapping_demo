using System;
using System.Collections.Generic;
using System.Globalization;

namespace CsvMapper.Models.Transformations
{
    /// <summary>
    /// Transformation that standardizes date formats
    /// </summary>
    public class DateFormatTransformation : TransformationBase
    {
        /// <summary>
        /// Gets the type of this transformation
        /// </summary>
        public override TransformationType Type => TransformationType.DateFormat;

        /// <summary>
        /// A collection of common date format strings
        /// </summary>
        public static readonly Dictionary<string, string> CommonDateFormats = new Dictionary<string, string>
        {
            { "ISO8601", "yyyy-MM-dd" },
            { "US", "MM/dd/yyyy" },
            { "European", "dd/MM/yyyy" },
            { "FileFriendly", "yyyyMMdd" },
            { "LongDate", "MMMM d, yyyy" },
            { "ShortDateWithDay", "ddd, MMM d, yyyy" }
        };

        /// <summary>
        /// A collection of common date formats for parsing
        /// </summary>
        private static readonly string[] CommonParseFormats = new[]
        {
            "yyyy-MM-dd", "yyyy/MM/dd", "MM/dd/yyyy", "dd/MM/yyyy", 
            "yyyyMMdd", "MMMM d, yyyy", "MMM d, yyyy",
            "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss"
        };

        /// <summary>
        /// Transforms a single input date value
        /// </summary>
        public override string Transform(string input, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string targetFormat = GetParameterValue(parameters, "TargetFormat", "yyyy-MM-dd");
            
            // Try parsing with various formats
            if (DateTime.TryParse(input, out DateTime date))
            {
                return date.ToString(targetFormat);
            }
            
            // Try with specific formats
            foreach (var format in CommonParseFormats)
            {
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString(targetFormat);
                }
            }
            
            // If all parsing attempts fail, return the original value
            return input;
        }

        /// <summary>
        /// Gets a user-friendly description of this transformation
        /// </summary>
        public override string GetDescription(Dictionary<string, object> parameters)
        {
            string targetFormat = GetParameterValue(parameters, "TargetFormat", "yyyy-MM-dd");
            string formatName = GetFormatNameForValue(targetFormat);
            
            return $"Format date as {formatName} ({targetFormat})";
        }

        /// <summary>
        /// Validates if the parameters are valid for this transformation
        /// </summary>
        public override bool ValidateParameters(Dictionary<string, object> parameters, out string error)
        {
            error = string.Empty;
            
            string targetFormat = GetParameterValue(parameters, "TargetFormat", "yyyy-MM-dd");
            
            try
            {
                // Make sure the format is valid by trying to format a sample date
                DateTime sampleDate = new DateTime(2023, 1, 31);
                sampleDate.ToString(targetFormat);
                return true;
            }
            catch (FormatException)
            {
                error = $"Invalid date format string: {targetFormat}";
                return false;
            }
        }
        
        /// <summary>
        /// Gets a friendly name for a format string if available
        /// </summary>
        private string GetFormatNameForValue(string formatValue)
        {
            foreach (var kvp in CommonDateFormats)
            {
                if (kvp.Value == formatValue)
                {
                    return kvp.Key;
                }
            }
            
            return "Custom";
        }
    }
}