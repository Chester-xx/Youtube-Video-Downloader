// OLD CODE ->

if (rgbCombined.IsChecked == true)
{

    // -> Need to implement a download and output tracker for both ytdlp and ffmpeg

    DeleteDownloadFiles();

    yt.OutputFolder = $@"Dependencies\";

    // yt-dlp -f "bestvideo[ext=webm]" --remux-video mp4 -o "video.mp4" "<VIDEO_URL>"

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
        ytdlpProcess.Start();
        ytdlpProcess.BeginOutputReadLine();
        ytdlpProcess.BeginErrorReadLine();
        ytdlpProcess.WaitForExit();
        YEC = ytdlpProcess.ExitCode;
    }

    if (YEC != 0)
    {
        MessageBox.Show($"YoutubeDL failed with exit code : {YEC}", "Error");
        sc1 = false;
        return;
    }

    ErrorStateAudio = await yt.RunAudioDownload(url: $@"{GetURL()}", progress: DownloadProgress, output: Output, format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3, overrideOptions: new YoutubeDLSharp.Options.OptionSet { Output = $@"Dependencies\daudio.mp3" });
    // create output file + dfiles, ffmpeg path and args for process
    string dvideo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "dvideo.mp4");
    string daudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "daudio.mp3");
    string outFile = config.UserInfo.Directory + "\\" + "output.mp4";

    if (!ErrorStateAudio.Success)
    {
        sc1 = false;
        return;
    }

    // -> merge video and audio using ffmpeg : output to directory
    ProcessStartInfo FFMpegProcessStartInfo = new ProcessStartInfo
    {
        FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dependencies", "ffmpeg.exe"),
        Arguments = $"-i \"{dvideo}\" -i \"{daudio}\" -c:v copy -c:a copy \"{outFile}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    // -> start process and wait for exit : show progress

    int FEC;

    using (Process ffmpegProcess = new Process { StartInfo = FFMpegProcessStartInfo })
    {
        ffmpegProcess.Start();
        ffmpegProcess.BeginOutputReadLine();
        ffmpegProcess.BeginErrorReadLine();
        ffmpegProcess.WaitForExit();
        FEC = ffmpegProcess.ExitCode;
    }

    DeleteDownloadFiles();

    if (ErrorStateAudio.Success)
    {
        sc1 = true;
        if (FEC != 0)
        {
            MessageBox.Show($"FFMpeg failed with exit code : {FEC}", "Error");
            sc1 = false;
        }
        if (YEC != 0)
        {
            MessageBox.Show($"YoutubeDL failed with exit code : {YEC}", "Error");
            sc1 = false;
        }
    }
}

//
//
//
//
// NEW CODE ->

