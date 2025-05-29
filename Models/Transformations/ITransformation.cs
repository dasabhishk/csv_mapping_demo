using System.Collections.Generic;

namespace CsvMapper.Models.Transformations
{
    /// <summary>
    /// Interface for all data transformations
    /// </summary>
    public interface ITransformation
    {
        /// <summary>
        /// Gets the type of this transformation
        /// </summary>
        TransformationType Type { get; }

        /// <summary>
        /// Transforms a single input value
        /// </summary>
        /// <param name="input">The input value to transform</param>
        /// <param name="parameters">Parameters for the transformation</param>
        /// <returns>The transformed value</returns>
        string Transform(string input, Dictionary<string, object> parameters);

        /// <summary>
        /// Transforms a single input value with default parameters
        /// </summary>
        /// <param name="input">The input value to transform</param>
        /// <returns>The transformed value</returns>
        string Transform(string input);

        /// <summary>
        /// Transforms a list of sample values
        /// </summary>
        /// <param name="inputs">The list of input values</param>
        /// <param name="parameters">Parameters for the transformation</param>
        /// <returns>The transformed values</returns>
        List<string> TransformSamples(List<string> inputs, Dictionary<string, object> parameters);

        /// <summary>
        /// Gets the current parameters for this transformation
        /// </summary>
        /// <returns>Dictionary of parameters</returns>
        Dictionary<string, object> GetParameters();

        /// <summary>
        /// Gets a user-friendly description of this transformation
        /// </summary>
        /// <param name="parameters">Parameters for the transformation</param>
        /// <returns>A description of the transformation</returns>
        string GetDescription(Dictionary<string, object> parameters);

        /// <summary>
        /// Validates if the parameters are valid for this transformation
        /// </summary>
        /// <param name="parameters">Parameters to validate</param>
        /// <param name="error">Error message if validation fails</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateParameters(Dictionary<string, object> parameters, out string error);
    }
}