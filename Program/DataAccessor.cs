using System;
using System.IO;
using System.Text.Json;

// DataAccessor class for managing user preferences and application settings
public class DataAccessor
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
            AppVersion = new Version(1, 2, 2, 0),
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

    public static void SetJSON(Preferences data)
    {
        File.WriteAllText(@"UserPreferences\preferences.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static Preferences GetJSON()
    {
        Preferences? data = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(@"UserPreferences\preferences.json"));

        if (data == null)
        {
            return config;
        }
        return data;
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
