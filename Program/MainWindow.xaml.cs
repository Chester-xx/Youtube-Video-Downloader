using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using System.Diagnostics;

namespace Program
{
    public partial class MainWindow : Window
    {
        

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
                DataAccessor.SetJSON(DataAccessor.config); 
                DataAccessor.CheckPref(); 
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
                Error.Display("Error", "Please enter a valid URL.");
                return;
            }
            // Did the user select a directory?
            if (String.IsNullOrEmpty(DataAccessor.config.UserInfo.Directory) || String.IsNullOrWhiteSpace(DataAccessor.config.UserInfo.Directory))
            {
                Error.Display("Error", "Please select a folder to store your download.");
                return;
            }

            // create instance of ytdl
            YoutubeDL yt = new()
            {
                YoutubeDLPath = @"Dependencies\yt-dlp.exe",
                FFmpegPath = @"Dependencies\ffmpeg.exe",
                OutputFolder = $@"{DataAccessor.config.DownloadOptionsInfo.Directory}"
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
                    Error.Display("Error", $"FFMpeg failed with exit code : {YEC}");
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
                string outFile = DataAccessor.config.UserInfo.Directory + "\\" + "output.mp4";

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
                        Error.Display("Error", $"FFMpeg failed with exit code : {FEC}");
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
                Error.Display("Error", "Please select a download method.");
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

                return await tcs.Task;
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

        private void BtnProfile(object sender, RoutedEventArgs e)
        {
            Profile.Display();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DataAccessor.SetJSON(DataAccessor.config);
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
                if (!string.IsNullOrEmpty(DataAccessor.config.UserInfo.Directory))
                {
                    lblDirectory.Content = DataAccessor.config.UserInfo.Directory;
                    return;
                }
                else
                {
                    lblDirectory.Content = "Download Destination : None";
                    return;
                }
            }

            DataAccessor.config.UserInfo.Directory = dialog.FolderName;
            lblDirectory.Content = DataAccessor.config.UserInfo.Directory;
            DataAccessor.SetJSON(DataAccessor.config);
        }

        // ON APP LOAD
        private void InitializeApplication()
        {
            DataAccessor.CheckPref();
            DataAccessor.config = DataAccessor.GetJSON();

            //check if dependency executables exist or directory itself
            if ((Directory.Exists(@"Dependencies") is bool dir && !dir) || !File.Exists(@"Dependencies\ffmpeg.exe") || !File.Exists(@"Dependencies\yt-dlp.exe"))
            {
                if (!dir)
                {
                    Directory.CreateDirectory(@"Dependencies");
                }
                DataAccessor.config.UserInfo.DependencyState = false;
            }

            // if dependencies are not installed : install
            if (!DataAccessor.config.UserInfo.DependencyState)
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
            if (!string.IsNullOrEmpty(DataAccessor.config.UserInfo.Directory) || !string.IsNullOrWhiteSpace(DataAccessor.config.UserInfo.Directory))
            {
                lblDirectory.Content = DataAccessor.config.UserInfo.Directory;
            }
        }
    }
}