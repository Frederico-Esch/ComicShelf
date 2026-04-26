using ComicShelfUI.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System.Data;
using System.IO;
using System.Windows;

namespace ComicShelfUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceCollection services = new ServiceCollection();

            ConfigureConfigurator(services);
            services.ConfigureRepositories();
            ConfigureWindows(services);

            serviceProvider = services.BuildServiceProvider();
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureConfigurator(ServiceCollection services)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var directory = Directory.GetCurrentDirectory();
            configurationBuilder.AddJsonFile($"{directory}\\appsettings.json");
            services.AddSingleton<IConfiguration>(configurationBuilder.Build());
        }

        private void ConfigureWindows(ServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MainWindow>();
            serviceCollection.AddTransient<AddCollection>();
            serviceCollection.AddTransient<InspectCollection>();
            serviceCollection.AddTransient<AddVolume>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (serviceProvider is IDisposable disposable)
                disposable.Dispose();
            base.OnExit(e);
        }
    }

}
