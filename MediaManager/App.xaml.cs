using MediaManager.Business;
using MediaManager.Data;
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
    public partial class App : Application
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
            services.AddSingleton(new DatabaseInitializer("movies.db"));
            services.AddSingleton<MovieRepository>();
            services.AddSingleton<MovieScanner>();
            services.AddSingleton<SettingsRepository>();
            services.AddSingleton<IUiLogger, UiLogger>();

            services.AddSingleton<MovieScanner>();
            // Injection de la fenêtre principale
            services.AddTransient<MainWindow>();
        }
    }
}