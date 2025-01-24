using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.IO.Enumeration;
using YoutubeDLSharp;
using Newtonsoft.Json;
using System.Net;
using System.Numerics;

namespace Program
{
    public partial class MainWindow : Window
    {
        // Globals
        string? DIR = null;

        public class Config
        {
            public string? Directory { get; set; }
            public bool DependencyState { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

    // BUTTONS & APP LOGIC

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

            var yt = new YoutubeDL();
            yt.YoutubeDLPath = @"C:\VideoDownloader\Dependencies\yt-dlp.exe";
            yt.FFmpegPath = @"C:\VideoDownloader\Dependencies\ffmpeg.exe";
            yt.OutputFolder = $@"{GetDir()}";

            var DownloadProgress = new Progress<DownloadProgress>((progress) => ShowProgress(progress));
            var Output = new Progress<string>((str) => lblOutput.Content = FormatStringOutput(str));

            var ErrorState = await yt.RunVideoDownload(url : $@"{GetURL()}", progress : DownloadProgress, output : Output, recodeFormat : YoutubeDLSharp.Options.VideoRecodeFormat.Mp4);

            if (ErrorState.Success)
            {
                txtURL.Text = string.Empty;
                
                return;
            }
            else
            {
                MessageBox.Show($"{ErrorState.ErrorOutput}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }

        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            SetDir();
        }

        private void txtURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            // main func for UX

            // set lblTitle.Content

            // set, viewcount, author, filesize, format, url







        }

    // PUBLIC AND PRIVATE FUNCTIONS

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

        private void ShowProgress(DownloadProgress progress)
        {
            prgbarDownload.Value = progress.Progress * 100;
            if (string.IsNullOrEmpty(progress.DownloadSpeed))
            {
                lblSpeed.Content = "0.00 MiB/s       00:00";
            }
            else
            {
                lblSpeed.Content = $"{progress.DownloadSpeed}       {progress.ETA}";
            }
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
        public object GetDir()
        {
            if (DIR == null)
            {
                return true;
            }
            return $@"{DIR}";
        }

        private void InitializeApplication()
        {
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
            if (string.IsNullOrEmpty(pref.Directory))
            {
                DIR = null;
            }
            else
            {
                DIR = pref.Directory;
                lblDirectory.Content = DIR;
            }            
        }
        
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
    }
}