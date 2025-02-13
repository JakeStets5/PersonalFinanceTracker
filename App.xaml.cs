using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Backend.Repositories;
using PersonalFinanceTracker.Backend.Services;
using PersonalFinanceTracker.Views;
using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Factories;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider? Services { get; private set; }


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);  // Call the base method to ensure WPF default startup behavior

            // Configure logging with Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()  // Configure Serilog to log to the console
                .CreateLogger();

            try
            {
                // Load DynamoDB credentials from Secrets Manager
                await AwsSecretsLoader.LoadDynamoDBCredentials();

                // Setup dependency injection (DI)
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);  // This method should add necessary services to the DI container

                // Build the service provider
                Services = serviceCollection.BuildServiceProvider();

                // Resolve the MainWindow from DI container
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Show();  // Show the MainWindow

                // Optional: If you need to log something at startup, you can log here
                Log.Information("Application started successfully.");
            }
            catch (Exception ex)
            {
                // Handle any initialization errors
                Log.Fatal(ex, "An error occurred during application startup.");
                throw;  // Rethrow exception to let the application crash if needed
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();  // Registering the user repo interface and its implementation
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddTransient<IAwsDynamoDbService, AwsDynamoDbService>();
            services.AddTransient<UserRepository>();  // Register UserRepository
            services.AddTransient<NavigationService>();  // Register UserRepository
            services.AddSingleton<DynamoDBContext>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<MainWindow>();  // Register MainWindow
            services.AddLogging();  // Add logging

            // DI for generating a new instance of the sign up window
            services.AddTransient<SignUpViewModel>();
            services.AddSingleton<IWindowFactory>(provider =>
                new WindowFactory(() => provider.GetRequiredService<SignUpViewModel>(), provider)
            );

            services.AddSingleton<NavigationService>();


        }
    }
}
