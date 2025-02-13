using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PersonalFinanceTracker.Backend.Repositories;
using PersonalFinanceTracker.ViewModels;
using PersonalFinanceTracker.Views;
using Serilog;

namespace PersonalFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserRepository _userRepository;

        public MainWindow(UserRepository userRepository)
        {
            try
            {
                _userRepository = userRepository;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while initializing MainWindow");
                throw;  // Rethrow or handle accordingly
            }
        }


        private void NavRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton clickedRadioButton)
            {
                // Get the position and size of the clicked RadioButton
                var position = clickedRadioButton.TransformToAncestor(this).Transform(new Point(0, 0));

                // Animate the underline's position and width to match the clicked button
                var positionAnimation = new DoubleAnimation
                {
                    From = Canvas.GetLeft(Underline),
                    To = position.X,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                var widthAnimation = new DoubleAnimation
                {
                    From = Underline.Width,
                    To = clickedRadioButton.ActualWidth,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                // Start the animations for position and width
                Underline.BeginAnimation(Canvas.LeftProperty, positionAnimation);
                Underline.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
            }
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var signInViewModel = new SignInViewModel(_userRepository);
            SignInWindow signInWindow = new SignInWindow(_userRepository, signInViewModel);
            signInWindow.ShowDialog();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Find the first RadioButton ("Home")
            if (NavStackPanel.Children[0] is RadioButton firstRadioButton)
            {
                // Calculate the position of the first RadioButton
                var position = firstRadioButton.TransformToAncestor(this).Transform(new Point(0, 0));

                // Set the initial position of the underline
                // Adjust the Y position to be below the button
                Canvas.SetLeft(Underline, position.X);
                Canvas.SetTop(Underline, position.Y + firstRadioButton.ActualHeight);
                Underline.Width = firstRadioButton.ActualWidth;

                // Ensure the underline is visible
                Underline.Visibility = Visibility.Visible;

                // Check the first button to match
                firstRadioButton.IsChecked = true;
            }
        }
    }
}