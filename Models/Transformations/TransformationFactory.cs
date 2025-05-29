using System.Collections.Generic;

namespace CsvMapper.Models.Transformations
{
    /// <summary>
    /// Factory class for creating common transformations with pre-configured parameters
    /// </summary>
    public static class TransformationFactory
    {
        /// <summary>
        /// Creates a transformation for extracting first names from full names
        /// </summary>
        /// <returns>Parameters for first name extraction</returns>
        public static Dictionary<string, object> FirstNameTransformation()
        {
            return new Dictionary<string, object>
            {
                { "Delimiter", " " }
            };
        }

        /// <summary>
        /// Creates a transformation for extracting last names from full names
        /// </summary>
        /// <returns>Parameters for last name extraction</returns>
        public static Dictionary<string, object> LastNameTransformation()
        {
            return new Dictionary<string, object>
            {
                { "Delimiter", " " }
            };
        }

        /// <summary>
        /// Creates a transformation for standardizing dates to ISO format
        /// </summary>
        /// <returns>Parameters for ISO date formatting</returns>
        public static Dictionary<string, object> IsoDateTransformation()
        {
            return new Dictionary<string, object>
            {
                { "TargetFormat", "yyyy-MM-dd" }
            };
        }

        /// <summary>
        /// Creates a transformation for standardizing gender values
        /// </summary>
        /// <returns>Parameters for gender standardization</returns>
        public static Dictionary<string, object> StandardGenderTransformation()
        {
            var genderMappings = new Dictionary<string, string>
            {
                { "M", "M" },
                { "Male", "M" },
                { "Man", "M" },
                { "Boy", "M" },
                { "F", "F" },
                { "Female", "F" },
                { "Woman", "F" },
                { "Girl", "F" },
                { "O", "O" },
                { "Other", "O" },
                { "Non-binary", "O" },
                { "U", "U" },
                { "Unknown", "U" },
                { "Not Specified", "U" },
                { "", "U" }
            };

            return new Dictionary<string, object>
            {
                { "Mappings", genderMappings },
                { "CaseSensitive", false },
                { "DefaultValue", "U" }
            };
        }

        /// <summary>
        /// Creates a transformation for extracting the year from a date
        /// </summary>
        /// <returns>Parameters for year extraction</returns>
        public static Dictionary<string, object> ExtractYearTransformation()
        {
            return new Dictionary<string, object>
            {
                { "TargetFormat", "yyyy" }
            };
        }
    }
}