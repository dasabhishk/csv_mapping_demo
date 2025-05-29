using CsvMapper.Services;
using System;

namespace CsvMapper
{
    /// <summary>
    /// Service locator for application-wide services
    /// </summary>
    public class ServiceLocator
    {
        private static ITransformationService? _transformationService;

        /// <summary>
        /// Gets the transformation service
        /// </summary>
        public static ITransformationService TransformationService
        {
            get
            {
                if (_transformationService == null)
                {
                    _transformationService = new TransformationService();
                }
                return _transformationService;
            }
        }
    }

    /// <summary>
    /// Provider for the transformation service (for XAML resources)
    /// </summary>
    public class TransformationServiceProvider
    {
        /// <summary>
        /// Gets the transformation service
        /// </summary>
        public ITransformationService ProvideService()
        {
            return ServiceLocator.TransformationService;
        }
    }
}