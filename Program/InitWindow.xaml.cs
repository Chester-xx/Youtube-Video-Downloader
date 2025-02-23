using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Program
{
    public partial class InitWindow : Window
    {
        bool check = false;

        public InitWindow()
        {
            InitializeComponent();
            Download();
            RunSequence();
        }

        private async void Download()
        {
            if (!File.Exists(@"Dependencies\yt-dlp.exe"))
            {
                await YoutubeDLSharp.Utils.DownloadYtDlp(@"Dependencies");
            }
            if (!File.Exists(@"Dependencies\ffmpeg.exe"))
            {
                await YoutubeDLSharp.Utils.DownloadFFmpeg(@"Dependencies");
            }
            check = true;
            UpdateDependencyState();
            await Animate("");
            this.Close();
        }

        private async void RunSequence()
        {
            await Task.Delay(2000);
            await Animate($"Let's Install Some\n   Dependencies");
            await Animate("This May Take a While");
            await Animate("");
            await StringAnimate();
        }

        private async Task Animate(string LabelText)
        {
            var col = new SolidColorBrush(Colors.White);
            var ColAnim = new ColorAnimation
            {
                From = Colors.White,
                To = Color.FromArgb(0, 255, 255, 255),
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = false
            };

            if (!check)
            {
                lblMain.Foreground = col;
                await Task.Delay(2000);
                col.BeginAnimation(SolidColorBrush.ColorProperty, ColAnim);
                await Task.Delay(3050);
                col.Color = Colors.White;
                lblMain.Content = LabelText;
            }
            else
            {
                lblMain.Foreground = col;
                await Task.Delay(3000);
                col.BeginAnimation(SolidColorBrush.ColorProperty, ColAnim);
                await Task.Delay(3500);
            }
        }

        private async Task StringAnimate()
        {
            var col = new SolidColorBrush(Colors.White);
            lblMain.Foreground = col;
            do
            {
                lblMain.Content = ".";
                await Task.Delay(1000);
                lblMain.Content = ". .";
                await Task.Delay(1000);
                lblMain.Content = ". . .";
                await Task.Delay(1000);

            }
            while (!check);
        }

        private void UpdateDependencyState()
        {
            MainWindow.CheckPref();
            MainWindow.Preferences data = MainWindow.GetJSON();
            data.UserInfo.DependencyState = true;
            MainWindow.SetJSON(data);
            MainWindow.config.UserInfo.DependencyState = true;
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
