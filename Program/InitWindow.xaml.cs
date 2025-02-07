using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json;

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
            await YoutubeDLSharp.Utils.DownloadYtDlp(@"C:\VideoDownloader\Dependencies");
            await YoutubeDLSharp.Utils.DownloadFFmpeg(@"C:\VideoDownloader\Dependencies");
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
            string path = @"C:\VideoDownloader\UserPreferences\preferences.json";
            var pref = new
            {
                Directory = "",
                DependencyState = true
            };
            string json = JsonConvert.SerializeObject(pref, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
