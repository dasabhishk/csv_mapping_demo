using CsvMapper.Models;
using CsvMapper.Models.Transformations;
using System;
using System.Collections.Generic;

namespace CsvMapper.Services
{
    /// <summary>
    /// Service for managing column transformations
    /// </summary>
    public class TransformationService : ITransformationService
    {
        private readonly Dictionary<TransformationType, ITransformation> _transformations;

        /// <summary>
        /// Constructor - initializes all available transformations
        /// </summary>
        public TransformationService()
        {
            _transformations = new Dictionary<TransformationType, ITransformation>
            {
                // Text transformations
                { TransformationType.SplitFirstToken, new SplitTextTransformation(TransformationType.SplitFirstToken) },
                { TransformationType.SplitLastToken, new SplitTextTransformation(TransformationType.SplitLastToken) },
                
                // Date transformations
                { TransformationType.DateFormat, new DateFormatTransformation() },
                
                // Category transformations
                { TransformationType.CategoryMapping, new CategoryMappingTransformation() },
                
                // Add more transformations as they are implemented
            };
        }

        /// <summary>
        /// Gets a transformation implementation by type
        /// </summary>
        public ITransformation GetTransformation(TransformationType type)
        {
            if (_transformations.TryGetValue(type, out var transformation))
            {
                return transformation;
            }
            
            throw new ArgumentException($"Transformation of type {type} is not supported.");
        }

        /// <summary>
        /// Creates a derived column by applying a transformation to a source column
        /// </summary>
        public DerivedColumn CreateDerivedColumn(
            CsvColumn sourceColumn, 
            string newColumnName, 
            TransformationType transformationType, 
            Dictionary<string, object> parameters)
        {
            // Validate parameters
            if (!ValidateTransformationParameters(transformationType, parameters, out string error))
            {
                throw new ArgumentException($"Invalid transformation parameters: {error}");
            }
            
            // Create the derived column
            var derivedColumn = DerivedColumn.Create(
                sourceColumn, 
                newColumnName, 
                transformationType, 
                parameters);
            
            // Apply the transformation to sample values
            var transformation = GetTransformation(transformationType);
            var transformedSamples = transformation.TransformSamples(sourceColumn.SampleValues, parameters);
            derivedColumn.SampleValues = transformedSamples;
            
            // Infer the type from the transformed samples
            derivedColumn.InferredType = InferColumnType(transformedSamples);
            
            return derivedColumn;
        }

        /// <summary>
        /// Applies a transformation to sample values
        /// </summary>
        public List<string> TransformSamples(
            List<string> sampleValues, 
            TransformationType transformationType, 
            Dictionary<string, object> parameters)
        {
            var transformation = GetTransformation(transformationType);
            return transformation.TransformSamples(sampleValues, parameters);
        }

        /// <summary>
        /// Validates transformation parameters
        /// </summary>
        public bool ValidateTransformationParameters(
            TransformationType transformationType, 
            Dictionary<string, object> parameters, 
            out string error)
        {
            var transformation = GetTransformation(transformationType);
            return transformation.ValidateParameters(parameters, out error);
        }

        /// <summary>
        /// Gets a description of a transformation with the given parameters
        /// </summary>
        public string GetTransformationDescription(
            TransformationType transformationType, 
            Dictionary<string, object> parameters)
        {
            var transformation = GetTransformation(transformationType);
            return transformation.GetDescription(parameters);
        }
        
        /// <summary>
        /// Infers the data type from sample values after transformation
        /// </summary>
        private string InferColumnType(List<string> sampleValues)
        {
            // This is a simplified version - in practice, you'd use the DataTypeHelper class
            // that already exists in your application
            
            // For demo purposes, here's a simple type inference
            bool allIntegers = true;
            bool allDecimals = true;
            bool allDates = true;
            
            foreach (var value in sampleValues)
            {
                if (string.IsNullOrEmpty(value))
                    continue;
                
                // Check for integer
                if (allIntegers && !int.TryParse(value, out _))
                    allIntegers = false;
                
                // Check for decimal
                if (allDecimals && !decimal.TryParse(value, out _))
                    allDecimals = false;
                
                // Check for date
                if (allDates && !DateTime.TryParse(value, out _))
                    allDates = false;
                
                // If nothing matches, no need to check further
                if (!allIntegers && !allDecimals && !allDates)
                    break;
            }
            
            if (allIntegers)
                return "int";
            if (allDecimals)
                return "decimal";
            if (allDates)
                return "datetime";
            
            return "string";
        }
    }
}