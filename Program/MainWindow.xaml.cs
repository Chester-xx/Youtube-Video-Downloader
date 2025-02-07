using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Program
{
    public partial class MainWindow : Window
    {
        // debouncer
        private readonly System.Timers.Timer debounceTimer = new(500) { AutoReset = false };

        // Globals
        string? DIR = null;
        // config for json file
        public class Config
        {
            public string? Directory { get; set; }
            public bool DependencyState { get; set; }
        }

        // Main window Init
        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();

            // debouncer
            debounceTimer.Elapsed += async (sender, e) => await Dispatcher.InvokeAsync(FetchVideoInfo);
        }

        // BUTTONS & APP LOGIC

        // User clicks on download button
        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            // Did the user enter a URL?
            if (GetURL() is bool noURL && noURL == true)
            {
                MessageBox.Show("Please enter a valid URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Did the user select a directory?
            if (GetDir() is bool noDir && noDir == true)
            {
                MessageBox.Show("Please select a folder for your download.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // create instance of ytdl
            YoutubeDL yt = new()
            {
                YoutubeDLPath = @"C:\VideoDownloader\Dependencies\yt-dlp.exe",
                FFmpegPath = @"C:\VideoDownloader\Dependencies\ffmpeg.exe",
                OutputFolder = $@"{GetDir()}"
            };

            lblStatus.Content = "Busy...";

            // create progress and output instances
            var DownloadProgress = new Progress<DownloadProgress>((progress) => ShowProgress(progress));
            var Output = new Progress<string>((str) => lblOutput.Content = FormatStringOutput(str));

            RunResult<string> ErrorState;
            RunResult<string> ErrorStateVideo;
            RunResult<string> ErrorStateAudio;

            // Standard
            if (rgbCombined.IsChecked == true)
            {
                ErrorState = await yt.RunVideoDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4);
            }

            // Video
            else if (rgbVideo.IsChecked == true)
            {
                ErrorState = await yt.RunVideoDownload(url: $@"{GetURL()}", overrideOptions: new YoutubeDLSharp.Options.OptionSet { Format = "bestvideo+bestaudio", PostprocessorArgs = new[] { "-an" } }, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, progress: DownloadProgress, output: Output);
            }

            // Audio
            else if (rgbAudio.IsChecked == true)
            {
                ErrorState = await yt.RunAudioDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
            }

            // Video + Audio
            else if (rgbVideoAndAudio.IsChecked == true)
            {
                ErrorStateVideo = await yt.RunVideoDownload(url: $@"{GetURL()}", overrideOptions: new YoutubeDLSharp.Options.OptionSet { Format = "bestvideo+bestaudio", PostprocessorArgs = new[] { "-an" } }, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, progress: DownloadProgress, output: Output);
                ErrorStateAudio = await yt.RunAudioDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
            }

            // No method selected for download
            else
            {
                MessageBox.Show("Please select a download method.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // NEED TO FIND WAY TO IMPLEMENT ERROR HANDLING AS CURRENT METHOD FAULTY AND NEEDED TO BE REMOVED
            // ErrorState.Success OR ErrorStateVideo/Audio.Success

            lblStatus.Content = "Done";
        }

        // User clicks on folder button
        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            // Set Directory in file storage and global var DIR
            SetDir();
        }

        // When user types in the URL textbox
        private async void txtURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            debounceTimer.Stop();
            debounceTimer.Start();
            //await FetchVideoInfo();
        }

        private async Task FetchVideoInfo()
        {
            // lets stop any resources from being used if there is no need
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                return;
            }

            // create instance of ytdl with dependency paths
            YoutubeDL yt = new()
            {
                YoutubeDLPath = @"C:\VideoDownloader\Dependencies\yt-dlp.exe",
                FFmpegPath = @"C:\VideoDownloader\Dependencies\ffmpeg.exe"
            };
            // get video information

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var video = await yt.RunVideoDataFetch($@"{txtURL.Text}");

                // if video and its data exist :
                if (video.Success)
                {
                    // create instance of video data
                    VideoData data = video.Data;

                    // set thumbnail using bitmap
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(data.Thumbnail);
                    bitmap.EndInit();
                    imgThumbNail.Source = bitmap;

                    // set labels
                    lblTitle.Content = $@"{data.Title}";
                    lblViewCount.Content = $@"Views : {data.ViewCount}";
                    lblAuthor.Content = $@"Author : {data.Uploader}";
                    lblLikeCount.Content = $@"Likes : {data.LikeCount}";
                    lblFormat.Content = $@"Format : {data.Format}";
                    lblURL.Content = $@"Link : {txtURL.Text}";
                }
                else
                {
                    // video and its data does not exist
                    ResetUI();
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void ResetUI()
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("https://i.imgur.com/v6tlkq8.jpeg");
            bitmap.EndInit();


            // clear all contents
            imgThumbNail.Source = bitmap;
            lblTitle.Content = string.Empty;
            lblViewCount.Content = "Views :";
            lblAuthor.Content = "Author :";
            lblLikeCount.Content = "Likes :";
            lblFormat.Content = "Format :";
            lblURL.Content = "Link :";
        }

        private void txtURL_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtURL.Text == "⌘ Paste Link Here")
            {
                txtURL.Text = string.Empty;
                txtURL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF197DCC"));
            }
        }

        private void txtURL_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                txtURL.Text = "⌘ Paste Link Here";
                txtURL.Foreground = Brushes.White;
            }
        }

        // PUBLIC AND PRIVATE FUNCTIONS

        // Formats string output for every 70 characters
        private static string FormatStringOutput(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            int max = 80;
            for (int i = max; i < str.Length; i += max + 1)
            {
                str = str.Insert(i, "\n");
            }
            return str;
        }

        // Updates download progress of videos
        private void ShowProgress(DownloadProgress progress)
        {
            // set progress bar

            prgbarDownload.Value = progress.Progress * 100;

            // UX, normally begins empty
            if (string.IsNullOrEmpty(progress.DownloadSpeed))
            {
                lblSpeed.Content = "0.00 MiB/s       00:00";
            }
            else // when its not empty output speed and ETA
            {
                lblSpeed.Content = $"{progress.DownloadSpeed}       {progress.ETA}";
            }
        }

        // gets URL from textbox
        public object GetURL()
        {
            // flag in logic
            if (string.IsNullOrEmpty(txtURL.Text) || !txtURL.Text.Contains("youtube", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return txtURL.Text;
        }

        // sets directory in file storage and updates global DIR
        public void SetDir()
        {
            // prompt file explorer
            OpenFolderDialog dialog = new OpenFolderDialog();
            bool? check = dialog.ShowDialog();

            // if no folder selected
            if (check == false)
            {
                return;
            }

            DIR = dialog.FolderName;

            // update contents of preference.json file, for when app is opened next time
            string path = @"C:\VideoDownloader\UserPreferences\preferences.json";
            var pref = new
            {
                Directory = $@"{DIR}",
                DependencyState = true
            };
            string json = JsonConvert.SerializeObject(pref, Formatting.Indented);
            File.WriteAllText(path, json);
            lblDirectory.Content = $@"{DIR}";
        }

        // gets global DIR
        public object GetDir()
        {
            // flag in logic
            if (DIR == null)
            {
                return true;
            }
            return $@"{DIR}";
        }

        // ON APP LOAD
        private void InitializeApplication()
        {
            // states of tests
            bool? DependencyState = null;
            bool? PreferenceState = null;

            // check if main dir of program exists
            if (!Directory.Exists(@"C:\VideoDownloader"))
            {
                Directory.CreateDirectory(@"C:\VideoDownloader");
                Directory.CreateDirectory(@"C:\VideoDownloader\Dependencies");
                Directory.CreateDirectory(@"C:\VideoDownloader\UserPreferences");
                DependencyState = false;
            }
            // check if sub dir exists
            if (!Directory.Exists(@"C:\VideoDownloader\Dependencies"))
            {
                Directory.CreateDirectory(@"C:\VideoDownloader\Dependencies");
                DependencyState = false;
            }
            // check if sub dir exists
            if (!Directory.Exists(@"C:\VideoDownloader\UserPreferences"))
            {
                Directory.CreateDirectory(@"C:\VideoDownloader\UserPreferences");
                PreferenceState = false;
            }
            // check if pref.json exists
            if (!File.Exists(@"C:\VideoDownloader\UserPreferences\preferences.json"))
            {
                PreferenceState = false;
            }

            // if preference folder did not exist : create json file
            if (PreferenceState == false)
            {
                string path = @"C:\VideoDownloader\UserPreferences\preferences.json";
                var pref2 = new
                {
                    Directory = "",
                    DependencyState = false
                };
                string json2 = JsonConvert.SerializeObject(pref2, Formatting.Indented);
                File.WriteAllText(path, json2);
                DIR = null;
            }

            // if preference.json exists : check dependency state
            if (DependencyState == null)
            {
                DependencyState = GetDependencyState();
                // if still null, C# logic ?? lines in code bracket still run
                if (DependencyState == null)
                {
                    MessageBox.Show("Preference Error...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            // if dependencies are not installed : install
            if (DependencyState == false)
            {
                // window prompt
                InitWindow initWindow = new InitWindow();
                initWindow.Show();
                this.Hide();
                initWindow.Closed += (sender, args) =>
                {
                    this.Visibility = Visibility.Visible;
                };
            }

            // init dir label and var
            string json = File.ReadAllText(@"C:\VideoDownloader\UserPreferences\preferences.json");
            var pref = JsonConvert.DeserializeObject<Config>(json);

            // if directory from json file empty
            if (string.IsNullOrEmpty(pref.Directory))
            {
                DIR = null;
            }
            else // otherwise set DIR
            {
                DIR = pref.Directory;
                lblDirectory.Content = DIR;
            }
        }

        // Gets Dependancy variable from json file
        private bool? GetDependencyState()
        {
            string path = Path.Combine(@"C:\VideoDownloader\UserPreferences", "preferences.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var pref = JsonConvert.DeserializeObject<Config>(json);
                return pref.DependencyState;
            }
            else
            {
                return null;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}