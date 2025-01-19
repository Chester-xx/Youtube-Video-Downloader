using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using YouTubeExplode;
using YouTubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        // Define the URL of the YouTube video you want to download
        string videoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"; // Replace with your video URL
        
        // Define the directory where you want to save the video
        string downloadFolder = @"C:\Users\YourUsername\Downloads"; // Change to your desired folder
        
        // Initialize the YouTubeExplode client
        var youtube = new YouTubeClient();
        
        try
        {
            // Get the video info
            var video = await youtube.Videos.GetAsync(videoUrl);
            
            // Get the video and audio streams
            var streamInfo = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            
            // Get the best video stream (no audio)
            var videoStream = streamInfo.GetVideoStreams().TryGetWithHighestVideoQuality();
            
            // Get the best audio stream
            var audioStream = streamInfo.GetAudioStreams().TryGetWithHighestBitrate();
            
            // Ensure the streams exist
            if (videoStream == null || audioStream == null)
            {
                Console.WriteLine("Error: No video or audio stream found.");
                return;
            }

            // Define file paths for video and audio
            string videoFilePath = Path.Combine(downloadFolder, $"{video.Title}_video.mp4");
            string audioFilePath = Path.Combine(downloadFolder, $"{video.Title}_audio.mp4");
            
            // Download video and audio streams
            Console.WriteLine($"Downloading video: {video.Title}...");
            await youtube.Videos.Streams.DownloadAsync(videoStream, videoFilePath);
            Console.WriteLine($"Downloading audio: {video.Title}...");
            await youtube.Videos.Streams.DownloadAsync(audioStream, audioFilePath);

            // Merge video and audio using ffmpeg
            string ffmpegPath = @"C:\path\to\ffmpeg.exe";  // Adjust this to the path where ffmpeg is installed
            string mergedFilePath = Path.Combine(downloadFolder, $"{video.Title}.mp4");

            // Construct the ffmpeg command
            string ffmpegArgs = $"-i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v copy -c:a aac -strict experimental \"{mergedFilePath}\"";
            
            // Run the ffmpeg command to merge the streams
            Console.WriteLine($"Merging video and audio...");
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process ffmpegProcess = Process.Start(processStartInfo);
            ffmpegProcess.WaitForExit();
            
            // Cleanup: Remove the temporary video and audio files
            File.Delete(videoFilePath);
            File.Delete(audioFilePath);

            Console.WriteLine($"Video downloaded and merged successfully! Saved at: {mergedFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}