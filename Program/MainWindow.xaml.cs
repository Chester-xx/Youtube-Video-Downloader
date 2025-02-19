// want to implement app dir and ect
// start menu shortcut addition
// desktop shortcut
// place actual exe in C:\\...\VideoDownloader
// maybe implement install when it does that? delete original exe on close? so app is placed inside folders and referenced to

using System.IO;
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
        // Globals
        private string? DIR = null;
        private bool Init = false;
        private bool TypeInit = false;

        // debounce timer
        private readonly System.Timers.Timer debounceTimer = new(1000) { AutoReset = false };

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

            this.Loaded += (s, e) => ClipCorners();
        }

        // BUTTONS & APP LOGIC

        // corner clipping/custom TItle Bar
        private void ClipCorners()
        {
            double CRad = 20;
            this.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight), CRad, CRad);
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            BtnDownload_Click(sender, e, lblStatus);
        }

        // User clicks on download button
        private async void BtnDownload_Click(object sender, RoutedEventArgs e, Label lblStatus)
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

            lblStatus.Content = "Downloading...";

            // create progress and output instances
            var DownloadProgress = new Progress<DownloadProgress>((progress) => ShowProgress(progress));
            var Output = new Progress<string>((str) => lblOutput.Content = FormatStringOutput(str));

            RunResult<string> ErrorState;
            RunResult<string> ErrorStateVideo;
            RunResult<string> ErrorStateAudio;
            bool sc1 = false;
            bool sc2 = false;
            bool sc3 = false;
            bool sc4 = false;

            // Standard
            if (rgbCombined.IsChecked == true)
            {
                ErrorState = await yt.RunVideoDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4);
                if (ErrorState.Success)
                {
                    sc1 = true;
                }
            }

            // Video
            else if (rgbVideo.IsChecked == true)
            {
                ErrorState = await yt.RunVideoDownload(url: $@"{GetURL()}", overrideOptions: new YoutubeDLSharp.Options.OptionSet { Format = "bestvideo+bestaudio", PostprocessorArgs = new[] { "-an" } }, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, progress: DownloadProgress, output: Output);
                if (ErrorState.Success)
                {
                    sc2 = true;
                }
            }

            // Audio
            else if (rgbAudio.IsChecked == true)
            {
                ErrorState = await yt.RunAudioDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
                if (ErrorState.Success)
                {
                    sc3 = true;
                }
            }

            // Video + Audio
            else if (rgbVideoAndAudio.IsChecked == true)
            {
                ErrorStateVideo = await yt.RunVideoDownload(url: $@"{GetURL()}", overrideOptions: new YoutubeDLSharp.Options.OptionSet { Format = "bestvideo+bestaudio", PostprocessorArgs = new[] { "-an" } }, recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, progress: DownloadProgress, output: Output);
                ErrorStateAudio = await yt.RunAudioDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
                if (ErrorStateVideo.Success && ErrorStateAudio.Success)
                {
                    sc4 = true;
                }
            }

            // No method selected for download
            else
            {
                MessageBox.Show("Please select a download method.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if any errors
            if (!sc1 && !sc2 && !sc3 && !sc4)
            {
                lblStatus.Content = "Failed";
            }
            else
            {
                lblStatus.Content = "Completed";
            }
        }

        // User clicks on folder button
        private void BtnDirectory_Click(object sender, RoutedEventArgs e)
        {
            // Set Directory in file storage and global var DIR
            SetDir();
        }

        // When user types in the URL textbox
        private void TxtURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (!TypeInit)
            {
                return;
            }

            if (!Init)
            {
                debounceTimer.Elapsed += async (s, ev) =>
                {
                    await Dispatcher.InvokeAsync(FetchVideoInfo);
                };
                Init = true;
            }

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // App drag method
        private void TitleBarDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Quit app logic
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private async Task FetchVideoInfo()
        {
            // lets stop any resources from being used if there is no need
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                lblStatus.Content = "Waiting for link...";
                imgThumbNail.Source = null;
                return;
            }
            if (txtURL.Text == "⌘ Paste Link Here")
            {
                lblStatus.Content = "Waiting for link...";
                imgThumbNail.Source = null;
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
                lblStatus.Content = "Fetching Video Info";
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
                    lblStatus.Content = "Ready to download";
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
            lblStatus.Content = "Failed";
        }

        private void TxtURL_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!TypeInit)
            {
                TypeInit = true;
            }

            if (txtURL.Text == "⌘ Paste Link Here")
            {
                txtURL.Text = string.Empty;
                txtURL.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF197DCC"));
            }
        }

        private void TxtURL_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                txtURL.Text = "⌘ Paste Link Here";
                txtURL.Foreground = Brushes.White;
            }
        }

        // PUBLIC AND PRIVATE FUNCTIONS

        // Formats string output for every 80 characters
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
                lblSpeed.Content = "0.00 MiB/s\t\t00:00";
            }
            else // when its not empty output speed and ETA
            {
                lblSpeed.Content = $"{progress.DownloadSpeed}\t{progress.ETA}";
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
            if (string.IsNullOrEmpty(value: pref.Directory))
            {
                DIR = null;
            }
            else // otherwise set DIR
            {
                DIR = pref.Directory;
                lblDirectory.Content = DIR;
            }

            // check for 

        }

        // Gets Dependancy variable from json file
        private static bool? GetDependencyState()
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
    }
}