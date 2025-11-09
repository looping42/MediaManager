using MediaManager.Application.Services;
using MediaManager.Domain.Interface;
using MediaManager.Domain.Movies.Interface;
using MediaManager.Infrastructure.Persistence;
using MediaManager.Logger;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MediaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Initialisation de la DB
            var dbInitializer = ServiceProvider.GetRequiredService<DatabaseInitializer>();
            dbInitializer.Initialize();

            // Résolution de la fenêtre principale via DI
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Exemple : injection de dépendances
            // Infrastructure
            services.AddSingleton(new DatabaseInitializer("movies.db"));
            //services.AddSingleton<DatabaseConnectionFactory>();

            // Repositories
            services.AddSingleton<MovieRepository>();
            services.AddSingleton<IMovieRepository, MovieRepository>();

            services.AddSingleton<ISettingsRepository, SettingsRepository>();

            // Logger
            services.AddSingleton<IUiLogger, UiLogger>();

            // Services Métadonnées
            services.AddSingleton<IMovieScanner, MovieScanner>();
            services.AddSingleton<IMovieMetadataProvider, TmdbMetadataProvider>();
            services.AddSingleton<IMovieMetadataProvider, FanartMetadataProvider>();

            // Injection de la fenêtre principale
            services.AddTransient<MainWindow>();
            services.AddTransient<SettingsWindow>();
        }
    }
}