using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Backend.Repositories;
using PersonalFinanceTracker.Backend.Services;
using PersonalFinanceTracker.Views;
using Serilog;
using System.Windows;
using Amazon.DynamoDBv2;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Factories;
using PersonalFinanceTracker.ViewModels;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

namespace PersonalFinanceTracker
{
    public partial class App : PrismApplication
    {
        public IServiceProvider? Services { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                await AwsSecretsLoader.LoadDynamoDBCredentials();

                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                Services = serviceCollection.BuildServiceProvider();

                // Register ILoggerFactory and IServiceProvider for dependency resolution
                Container.GetContainer().RegisterInstance(Services.GetRequiredService<ILoggerFactory>());
                Container.GetContainer().RegisterInstance<IServiceProvider>(Services);

                Log.Information("Application started successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred during application startup.");
                throw;
            } 
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>(); // Showing the main window with prism
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // View and ViewModel registrations
            containerRegistry.RegisterForNavigation<UploadTransactionView>();
            containerRegistry.Register<SignUpViewModel>();
            containerRegistry.Register<SignInViewModel>();
            containerRegistry.Register<MainWindow>();

            // Main window dependencies
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.Register<IUserRepository, UserRepository>();
            containerRegistry.Register<INavigationService, NavigationService>();
            containerRegistry.Register<IPFTDialogService, PFTDialogService>();
            containerRegistry.Register<IUserSessionService, UserSessionService>();  

            // Service and AWS registrations
            containerRegistry.Register<ICloudDbService, AwsDynamoDbService>();
            containerRegistry.RegisterInstance<IAmazonDynamoDB>(new AmazonDynamoDBClient());
            containerRegistry.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
            containerRegistry.RegisterSingleton<IUserSessionService, UserSessionService>();
            containerRegistry.Register<IFinancialDataService, FinancialDataService>();

            // WindowFactory with deferred SignUpViewModel resolution
            containerRegistry.RegisterInstance<IWindowFactory>(
                new WindowFactory(() => Container.Resolve<SignUpViewModel>(), () => Container.Resolve<SignInViewModel>(), Services)
            );
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // View and ViewModel services
            services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>();
            services.AddSingleton<IPFTDialogService, PFTDialogService>();

            // Stateless backend services
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICloudDbService, AwsDynamoDbService>();
            services.AddTransient<IFinancialDataService, FinancialDataService>();
            services.AddTransient<IUserSessionService, UserSessionService>();

            // Shared single-instance services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

            // Logging service
            services.AddLogging();
        }
    }
}
