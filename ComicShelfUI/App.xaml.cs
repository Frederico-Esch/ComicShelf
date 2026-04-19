using ComicShelfUI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System.Configuration;
using System.Data;
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
            services.ConfigureRepositories();
            ConfigureWindows(services);
            serviceProvider = services.BuildServiceProvider();
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureWindows(ServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MainWindow>();
            serviceCollection.AddTransient<AddCollection>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (serviceProvider is IDisposable disposable)
                disposable.Dispose();
            base.OnExit(e);
        }
    }

}
