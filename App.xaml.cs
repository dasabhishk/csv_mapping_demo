using CsvMapper.Models;
using CsvMapper.Services;
using CsvMapper.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;

namespace CsvMapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<ICsvParserService, CsvParserService>();
            services.AddSingleton<ISchemaLoaderService, SchemaLoaderService>();
            services.AddSingleton<ITransformationService, TransformationService>();
            services.AddSingleton<IMappingService>(sp => new MappingService(sp.GetRequiredService<ITransformationService>()));

            // Register viewmodels
            services.AddSingleton<MainViewModel>();

            // Register main window
            services.AddSingleton<MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
}
