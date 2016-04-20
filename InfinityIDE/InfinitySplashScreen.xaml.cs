using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace InfinityIDE
{
    /// <summary>
    /// Interaction logic for InfinitySplashScreen.xaml
    /// </summary>
    public partial class InfinitySplashScreen : Window
    {
        Storyboard storyboard;

        public InfinitySplashScreen()
        {
            InitializeComponent();
            this.board.Completed += OnAnimationCompleted;
        }

        #region ISplashScreen
        public void Progress(double value)
        {
            progressBar.Value = value;
        }
        public void CloseSplashScreen()
        {
            this.board.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(500));
            this.board.Begin(this);
        }
        public void SetProgressState(bool isIndeterminate)
        {
            progressBar.IsIndeterminate = isIndeterminate;
        }
        #endregion

        #region Event Handlers
        void OnAnimationCompleted(object sender, EventArgs e)
        {
            this.board.Completed -= OnAnimationCompleted;
            this.Close();
        }
        #endregion

        private void Splash_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void splashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow = new InfinityMain();
            progressBar.IsIndeterminate = false;
            DoubleAnimation value = new DoubleAnimation()
            {
                From = 0,
                To = 100,
                EasingFunction = new QuinticEase(),
                Duration = new Duration(TimeSpan.FromSeconds(4))
            };
            Storyboard.SetTarget(value, progressBar);
            Storyboard.SetTargetProperty(value, new PropertyPath(ProgressBar.ValueProperty));
            storyboard = new Storyboard();
            storyboard.Children.Add(value);
            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
        }

        void storyboard_Completed(object sender, EventArgs e)
        {
            CloseSplashScreen();
            App.MainWindow.Show();
        }
    }
}
