using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using YoutubeDLSharp;

namespace Program
{// NBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB!!!
// PLEEASE SET DEPENDANCYSTATE to TRUE in JSON FILE
    public partial class InitWindow : Window
    {
        public InitWindow()
        {
            InitializeComponent();
            Download();
            RunSequence();
        }

        private async void Download()
        {
            await YoutubeDLSharp.Utils.DownloadYtDlp(@"C:\VideoDownloader\Dependancies");
            await YoutubeDLSharp.Utils.DownloadFFmpeg(@"C:\VideoDownloader\Dependancies");
        }

        private async void RunSequence()
        {
            await Task.Delay(3000);
            await Animate("Let's Install Some Dependencies");
            await Task.Delay(3000);
            await Animate("This May Take a While");
            await Task.Delay(3000);
            await Animate("...");
        }

        private async Task Animate(string LabelText)
        {
            var ColAnim = new ColorAnimation
            {
                From = Colors.White,
                To = Color.FromArgb(0, 255, 255, 255),
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = false
            };
            var col = new SolidColorBrush(Colors.White);
            lblMain.Foreground = col;
            col.BeginAnimation(SolidColorBrush.ColorProperty, ColAnim);
            await Task.Delay(3000);
            col.Color = Colors.White;
            lblMain.Content = LabelText;
        }

    }
}
