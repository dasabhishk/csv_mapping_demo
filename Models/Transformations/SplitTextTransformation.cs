using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvMapper.Models.Transformations
{
    /// <summary>
    /// Transformation that splits text and extracts specific tokens
    /// </summary>
    public class SplitTextTransformation : TransformationBase
    {
        /// <summary>
        /// The type of split operation
        /// </summary>
        private readonly TransformationType _splitType;

        /// <summary>
        /// Constructor
        /// </summary>
        public SplitTextTransformation(TransformationType splitType)
        {
            if (splitType != TransformationType.SplitFirstToken && 
                splitType != TransformationType.SplitLastToken)
            {
                throw new ArgumentException("Invalid split transformation type", nameof(splitType));
            }

            _splitType = splitType;
        }

        /// <summary>
        /// Gets the type of this transformation
        /// </summary>
        public override TransformationType Type => _splitType;

        /// <summary>
        /// Transforms a single input value
        /// </summary>
        public override string Transform(string input, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string delimiter = GetParameterValue(parameters, "Delimiter", " ");
            
            var parts = input.Split(new[] { delimiter }, StringSplitOptions.None);
            
            if (parts.Length == 0)
            {
                return string.Empty;
            }
            
            if (_splitType == TransformationType.SplitFirstToken)
            {
                return parts[0];
            }
            else // SplitLastToken
            {
                return parts[parts.Length - 1];
            }
        }

        /// <summary>
        /// Gets a user-friendly description of this transformation
        /// </summary>
        public override string GetDescription(Dictionary<string, object> parameters)
        {
            string delimiter = GetParameterValue(parameters, "Delimiter", " ");
            string delimiterDisplay = delimiter == " " ? "space" : $"'{delimiter}'";
            
            if (_splitType == TransformationType.SplitFirstToken)
            {
                return $"Extract first part before {delimiterDisplay}";
            }
            else // SplitLastToken
            {
                return $"Extract last part after {delimiterDisplay}";
            }
        }

        /// <summary>
        /// Validates if the parameters are valid for this transformation
        /// </summary>
        public override bool ValidateParameters(Dictionary<string, object> parameters, out string error)
        {
            error = string.Empty;
            
            // The delimiter parameter is optional (defaults to space), so there's nothing to validate
            return true;
        }
    }
}