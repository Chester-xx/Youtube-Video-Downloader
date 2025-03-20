if (rgbCombined.IsChecked == true)
{
    DeleteDownloadFiles();
    yt.OutputFolder = @"Dependencies\";

    lblOutput.Content = "Downloading video...";
    prgbarDownload.Value = 0;

    var DownloadProgress_ = new Progress<double>((progress) => ShowYTDLProgress(progress));

    ProcessStartInfo ytdlpInfo = new ProcessStartInfo()
    {
        FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "yt-dlp.exe"),
        Arguments = $"-f bestvideo[ext=webm] --remux-video mp4 -o Dependencies\\dvideo.mp4 {GetURL()}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    int YEC;

    using (Process ytdlpProcess = new Process { StartInfo = ytdlpInfo })
    {
        ytdlpProcess.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lblOutput.Dispatcher.Invoke(() => lblOutput.Content = $"yt-dlp: {args.Data}");

                Match match = Regex.Match(args.Data, @"\[download\] (\d+(\.\d+)?)%");
                if (match.Success)
                {
                    double progressValue = double.Parse(match.Groups[1].Value);
                    ((IProgress<double>)DownloadProgress_).Report(progressValue);
                }
            }
        };

        ytdlpProcess.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                lblOutput.Dispatcher.Invoke(() => lblOutput.Content = $"yt-dlp Error: {args.Data}");
        };

        ytdlpProcess.Start();
        ytdlpProcess.BeginOutputReadLine();
        ytdlpProcess.BeginErrorReadLine();
        ytdlpProcess.WaitForExit();
        YEC = ytdlpProcess.ExitCode;
    }

    ((IProgress<double>)DownloadProgress).Report(100);

    if (YEC != 0)
    {
        MessageBox.Show($"yt-dlp failed with exit code: {YEC}", "Error");
        sc1 = false;
        return;
    }

    lblOutput.Content = "Processing completed.";
}


// 
// 
// 
// 