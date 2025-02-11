using System;

// all methods would be called under the RoutedEventArgs of the btnMinimize call

// USER MINIMIZES USING BUTTON
// minimize() -> ResetWindow()

// USER MAXIMISES USING TASK BAR
// WindowStateChange()

// USER MINIMIZES USING TASK BAR ??????????????????
// WindowStateChange() ?? Failure

public class Class1
{
	public Class1()
    {
        // Failed animation implementation

        private async void Minimize()
        {
            // pointer to original values before changes
            _height = this.ActualHeight;
            _opacity = this.Opacity;

            DoubleAnimation heightAnimation = new()
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation opacityAnimation = new()
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.HeightProperty, heightAnimation);
            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);

            await Task.Delay(350);
            // remove the animation privileges
            this.BeginAnimation(Window.HeightProperty, null);
            this.BeginAnimation(Window.OpacityProperty, null);

            await ResetWindow();
        }

        async Task ResetWindow()
        {
            await Task.Delay(400);
            this.WindowState = WindowState.Minimized;
            this.Height = _height;
            this.Opacity = _opacity;
        }

        private void WindowStateChange(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                Dispatcher.BeginInvoke((Action)(async () =>
                {
                    // remove previous animation privileges
                    this.BeginAnimation(Window.HeightProperty, null);
                    this.BeginAnimation(Window.OpacityProperty, null);

                    DoubleAnimation heightAnimation = new()
                    {
                        From = 0,
                        To = _height,
                      Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    DoubleAnimation opacityAnimation = new()
                    {
                        From = 0,
                        To = _opacity,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    // begin new animation
                    this.BeginAnimation(Window.HeightProperty, heightAnimation);
                    this.BeginAnimation(Window.OpacityProperty, opacityAnimation);

                    await Task.Delay(350);
                    // remove the animation
                    this.BeginAnimation(Window.HeightProperty, null);
                    this.BeginAnimation(Window.OpacityProperty, null);

                    // reset height and opacity
                    this.Height = _height;
                    this.Opacity = _opacity;

                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            else if (this.WindowState == WindowState.Minimized)
            {
            // breaker ??
            // worked up until addition of this call, this call was made because the animation would
            // not occur if the user minimized it from the task bar.

            //Minimize();
        }
    }
    }
}
