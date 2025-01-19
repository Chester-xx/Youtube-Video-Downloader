using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using Microsoft.Win32;
using YoutubeExplode.Videos.Streams;
using System.IO;

namespace Program
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Globals
        string? DIR;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            // Did the user enter a URL?
            if (GetURL() is bool noURL && noURL == true)
            {
                MessageBox.Show("Please enter a valid URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // If so, proceed
            var URL = Convert.ToString(GetURL());

            // Did the user select a directory?
            if (GetDir() is bool noDir && noDir == true)
            {
                MessageBox.Show("Please select a folder for your download.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // If so, proceed
            var DIR = Convert.ToString(GetDir());

            // create new instance of dependancy
            YoutubeClient yt = new YoutubeClient();

            try
            {
                // get video information and high quality stream information
                var video = await yt.Videos.GetAsync(URL);
                var videoInfo = await yt.Videos.Streams.GetManifestAsync(video.Id);
                var stream = videoInfo.GetVideoStreams().TryGetWithHighestVideoQuality();

                if (stream == null)
                {
                    MessageBox.Show("Video Stream not available or not found.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // establish path
                DIR = Path.Combine(DIR, $"{video.Title}.mp4");

                //thread task - download video
                await yt.Videos.Streams.DownloadAsync(stream, DIR);
                MessageBox.Show("Download Complete", "Finished", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured while downloading the video. {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            SetDir();
        }

        public object GetURL()
        {
            // Test URL
            if (string.IsNullOrEmpty(txtURL.Text) || !txtURL.Text.Contains("youtube", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return txtURL.Text;
        }

        public void SetDir()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            bool? check = dialog.ShowDialog();

            if (check == false)
            {
                return;
            }
            DIR = dialog.FolderName;
        }
        public object GetDir()
        {
            if (DIR == null)
            {
                return true;
            }
            return Convert.ToString(DIR);
        }
    }
}