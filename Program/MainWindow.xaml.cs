using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using System.Text.Json;
using YoutubeDLSharp.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Program
{
    public partial class MainWindow : Window
    {
        //config structure
        public class Preferences
        {
            public required ProgramInfo ProgramInfo { get; set; }
            public required User UserInfo { get; set; }
            public required DownloadOptions DownloadOptionsInfo { get; set; }
        }
        public class ProgramInfo
        {
            public required Version AppVersion { get; set; }
            public required string Website { get; set; }
            public required string Developer { get; set; }
            public required string Language { get; set; }
        }
        public class User
        {
            public required string Directory { get; set; }
            public required bool DependencyState { get; set; }
        }
        public class DownloadOptions
        {
            public required string Directory { get; set; }
            public required string AudioQuality { get; set; }
            public required string VideoQuality { get; set; }

        }

        // Config
        public static Preferences config = new()
        {
            ProgramInfo = new ProgramInfo
            {
                AppVersion = new Version(1, 2, 0, 1),
                Website = "http://github.com/Chester-xx/Youtube-Video-Downloader",
                Developer = "Chester-xx",
                Language = "en-US"
            },
            UserInfo = new User
            {
                Directory = string.Empty,
                DependencyState = true
            },
            DownloadOptionsInfo = new DownloadOptions
            {
                Directory = string.Empty,
                AudioQuality = string.Empty,
                VideoQuality = string.Empty
            }
        };

        // Globals
        private bool Init = false;
        private bool TypeInit = false;

        // debounce timer
        private readonly System.Timers.Timer debounceTimer = new(1000) { AutoReset = false };

        // Main window Init
        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
            this.Loaded += (s, e) => ClipCorners();
            this.Closing += (s, e) => 
            { 
                SetJSON(config); 
                CheckPref(); 
            };
        }

        // BUTTONS & APP LOGIC

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // if any new additions need to be made in the future
            // handles events on app close
        }

        // corner clipping/custom Title Bar
        private void ClipCorners()
        {
            double CRad = 25;
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
            if (GetURL() is bool noURL && noURL)
            {
                MessageBox.Show("Please enter a valid URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Did the user select a directory?
            if (String.IsNullOrEmpty(config.UserInfo.Directory) || String.IsNullOrWhiteSpace(config.UserInfo.Directory))
            {
                MessageBox.Show("Please select a folder for your download.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // create instance of ytdl
            YoutubeDL yt = new()
            {
                YoutubeDLPath = @"Dependencies\yt-dlp.exe",
                FFmpegPath = @"Dependencies\ffmpeg.exe",
                OutputFolder = $@"{config.DownloadOptionsInfo.Directory}"
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

            // delete any previous download files
            static void DeleteDownloadFiles()
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "dvideo.mp4")))
                {
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "dvideo.mp4"));
                }
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "daudio.mp3")))
                {
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "daudio.mp3"));
                }
            }

            // Standard
            if (rgbCombined.IsChecked == true)
            {
                // delete previous files for cleanup
                DeleteDownloadFiles();

                yt.OutputFolder = $@"Dependencies\";

                // processtart info for ytdlp.exe
                ProcessStartInfo ytdlpInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "yt-dlp.exe"),
                    Arguments = $"-f bestvideo[ext=webm]/bestvideo --remux-video mp4 -o Dependencies\\dvideo.mp4 {GetURL()}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // exit code ytdlp
                int YEC = await RunProcessAsync(ytdlpInfo);

                // error
                if (YEC != 0)
                {
                    MessageBox.Show($"YoutubeDL failed with exit code : {YEC}", "Error");
                    sc1 = false;
                    return;
                }

                // run audio download
                ErrorStateAudio = await yt.RunAudioDownload(
                    url: $@"{GetURL()}",
                    progress: DownloadProgress,
                    output: Output,
                    format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3,
                    overrideOptions: new YoutubeDLSharp.Options.OptionSet 
                    { 
                        Output = $@"Dependencies\daudio.mp3" 
                    }
                );

                // success state check
                if (!ErrorStateAudio.Success)
                {
                    sc1 = false;
                    return;
                }

                // create output file + dfiles, ffmpeg path and args for process
                string dvideo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "dvideo.mp4");
                string daudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "daudio.mp3");
                string outFile = config.UserInfo.Directory + "\\" + "output.mp4";

                // processstart info for ffmpeg.exe
                ProcessStartInfo FFMpegProcessStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "ffmpeg.exe"),
                    Arguments = $"-i \"{dvideo}\" -i \"{daudio}\" -c:v copy -c:a copy \"{outFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // exit code ffmpeg
                int FEC = await RunProcessAsync(FFMpegProcessStartInfo);

                // remove current temp files
                DeleteDownloadFiles();

                // success state check
                if (ErrorStateAudio.Success)
                {
                    sc1 = true;
                    if (FEC != 0)
                    {
                        MessageBox.Show($"FFMpeg failed with exit code : {FEC}", "Error");
                        sc1 = false;
                    }
                }
            }

            // Video
            else if (rgbVideo.IsChecked == true)
            {
                ErrorState = await yt.RunVideoDownload(
                    url: $@"{GetURL()}", 
                    overrideOptions: new YoutubeDLSharp.Options.OptionSet 
                        { 
                            Format = "bestvideo+bestaudio", 
                            PostprocessorArgs = new[] { "-an" } 
                        }, 
                    recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, 
                    progress: DownloadProgress, 
                    output: Output
                );
                
                if (ErrorState.Success)
                {
                    sc2 = true;
                }
            }

            // Audio
            else if (rgbAudio.IsChecked == true)
            {
                ErrorState = await yt.RunAudioDownload(
                    url: $@"{GetURL()}", 
                    progress: DownloadProgress, 
                    output: Output, 
                    format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3
                );
                
                if (ErrorState.Success)
                {
                    sc3 = true;
                }
            }

            // Video + Audio
            else if (rgbVideoAndAudio.IsChecked == true)
            {
                ErrorStateVideo = await yt.RunVideoDownload(
                    url: $@"{GetURL()}", 
                    overrideOptions: new YoutubeDLSharp.Options.OptionSet 
                        { 
                            Format = "bestvideo+bestaudio", 
                            PostprocessorArgs = new[] { "-an" } 
                        }, 
                    recodeFormat: YoutubeDLSharp.Options.VideoRecodeFormat.Mp4, 
                    progress: DownloadProgress, 
                    output: Output
                );
                
                ErrorStateAudio = await yt.RunAudioDownload(
                    url: $@"{GetURL()}", 
                    progress: DownloadProgress, 
                    output: Output, 
                    format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3
                );
                
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

        private async Task<int> RunProcessAsync(ProcessStartInfo startInfo)
        {
            using (Process process = new Process { StartInfo = startInfo })
            {
                var tcs = new TaskCompletionSource<int>();

                process.EnableRaisingEvents = true;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Dispatcher.Invoke(() => lblOutput.Content = e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Dispatcher.Invoke(() => lblOutput.Content = $"Error: {e.Data}");
                };


                process.Exited += (sender, e) => tcs.SetResult(process.ExitCode);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return await tcs.Task; // Asynchronously wait for process to exit
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
            SetJSON(config);
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

                lblTitle.Content = string.Empty;
                lblViewCount.Content = "Views :";
                lblAuthor.Content = "Author :";
                lblLikeCount.Content = "Likes :";
                lblFormat.Content = "Format :";
                lblURL.Content = "Link :";

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
                YoutubeDLPath = @"Dependencies\yt-dlp.exe",
                FFmpegPath = @"Dependencies\ffmpeg.exe"
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

        private void ShowYTDLProgress(double data)
        {
            lblOutput.Content = data.ToString();
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

        // sets directory in file storage and updates config preference class attribute
        public void SetDir()
        {
            // prompt file explorer
            OpenFolderDialog dialog = new OpenFolderDialog();
            bool? check = dialog.ShowDialog();

            // if no folder selected
            if (check == false)
            {
                lblDirectory.Content = "Download Destination : None";
                return;
            }

            config.UserInfo.Directory = dialog.FolderName;
            lblDirectory.Content = config.UserInfo.Directory;
            SetJSON(config);
        }

        // ON APP LOAD
        private void InitializeApplication()
        {
            CheckPref();
            config = GetJSON();

            //check if dependency executables exist or directory itself
            if ((Directory.Exists(@"Dependencies") is bool dir && !dir) || !File.Exists(@"Dependencies\ffmpeg.exe") || !File.Exists(@"Dependencies\yt-dlp.exe"))
            {
                if (!dir)
                {
                    Directory.CreateDirectory(@"Dependencies");
                }
                config.UserInfo.DependencyState = false;
            }

            // if dependencies are not installed : install
            if (!config.UserInfo.DependencyState)
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

            // if directory from json file empty
            if (!string.IsNullOrEmpty(config.UserInfo.Directory) || !string.IsNullOrWhiteSpace(config.UserInfo.Directory))
            {
                lblDirectory.Content = config.UserInfo.Directory;
            }
        }

        // returns a static accessible Preference class of stored file, which is basically just the contents of the preferences.json file
        public static Preferences GetJSON()
        {
            Preferences? data = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(@"UserPreferences\preferences.json"));
            if (data == null)
            {
                return config;
            }
            return data;
        }

        public static void SetJSON(Preferences data)
        {
            File.WriteAllText(@"UserPreferences\preferences.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void CheckPref()
        {
            // ensure directories and files exist before accessing them
            if (!Directory.Exists(@"UserPreferences") || !File.Exists(@"UserPreferences\preferences.json"))
            {
                Directory.CreateDirectory(@"UserPreferences");
                SetJSON(config);
            }
        }
    }
}