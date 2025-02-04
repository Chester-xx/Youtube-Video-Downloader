import os, sys, json, requests, subprocess, yt_dlp, tkinter as tk
from tkinter import filedialog, messagebox

# On App Start-Up, Check for user's folder preference, ffmpeg presence and lastly environment variables
def start_test() -> None :
    """
    Once the application is ran:
    
    1. Check for user's folder preference:
        - Check if folder "vtemp" exists. If not create it.
        - Check if json file "dir.json" exists. If not create it.
    2. Check for users selected folder:
        - Check if a previous directory has been stored. If so, set the value to directory_var.
        - If a previous directory has not been stored, write it to the "dir.json" file. \n
        directory_var stores the current active folder for downloads.
    3. Get FFMpeg:
        - Checks and installation.
    4. Update system environment variables:
        - Checks and sets paths for dependancies.
    """
    path = os.path.join("C:\\", "vtemp", "dir.json")
    if not os.path.exists(os.path.join("C:\\", "vtemp")) :
        os.makedirs(os.path.join("C:\\", "vtemp"))
    if not os.path.isfile(path) :
        with open(path, "w") as json_f :
            json.dump({"dir" : directory_var.get()}, json_f)
    else :
        with open(path, "r") as json_f :
            content = json.load(json_f)
        directory_var.set(content["dir"])
    Get_ffmpeg()
    Update_Var()

# Checks presence of FFMpeg
def Get_ffmpeg() -> None :
    """
    1. Checks if FFMpeg is already installed, if so stops.
    2. Download the 7z FFMpeg File
        - Set the file names and paths.
        - Get the download link, if successful download the file.
    3. Unzip Content
        - Set an absolute path to file.
        - Create a directory for the dependancy to be stored on the main drive.
        - Run it under a subprocess using 7z File Manager.
    4. Remove zipped content
        - Delete zip content from system.
    """
    if os.path.isdir(f"C:\\ffmpeg") :
        return None
    # download the 7z file
    messagebox.showwarning("Downloading Dependancy : FFMpeg", "Please wait a few moments")
    file_name = os.path.basename(r"ffmpeg-git-full.7z")
    res = requests.get("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z")
    if (res.status_code == 200) :
        with open(file_name, "wb") as file :
            file.write(res.content)
    else : messagebox.showerror("Error", "Failed to download dependancy : FFMpeg.") ; quit()
    # unzip to Main drive
    cur_dir = os.path.dirname(os.path.abspath(__file__))
    os.makedirs(r"C:\ffmpeg", exist_ok = True)
    subprocess.run(["7z", "x", os.path.join(cur_dir, "ffmpeg-git-full.7z"), f"-oC:\\ffmpeg"], check = True, text = True)
    os.remove(os.path.join(os.path.dirname(os.path.abspath(__file__)), "ffmpeg-git-full.7z"))
    messagebox.showinfo("Success", "Finished installing FFMpeg")

# Updates and adds dependancies to the windows environment system variables path
def Update_Var() -> None :
    """
    1. Sets both path dependancies
        - Both paths will be needed for function, path2 gets the version name of ffmpeg
    2. Set
        - Sets the paths in the windows system so python can actively call on each method \n
        without having to specify the path of the binaries and their files
    """
    path1 = r"C:\Program Files\7-Zip"
    items = os.listdir(r"C:\\ffmpeg")
    path2 = f"C:\\ffmpeg\\" + items[0] + r"\bin"
    def Set(path) -> None :
        current_path = os.environ.get('PATH', '')
        if (path not in current_path) :
            new_path_variable = current_path + ';' + path
            os.environ['PATH'] = new_path_variable
            if (sys.platform == "win32") :
                import winreg as reg
                try:
                    reg_key = reg.OpenKey(reg.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", 0, reg.KEY_SET_VALUE)
                    reg.SetValueEx(reg_key, "Path", 0, reg.REG_EXPAND_SZ, new_path_variable)
                    reg.CloseKey(reg_key)
                    print(f"Successfully added {path} to the PATH environment variable.")
                except Exception as e:
                    messagebox.showerror("Error", f"Failed to update PATH in the registry: {e}, please run program in administrator mode. If you have ran the application in administrator mode, you must restart your pc for changes to be made.")
        else:
            print(f"{path} is already in the PATH environment variable.")
    Set(path1)
    Set(path2)

# gets the users chosen directory and dumps it to a "temp" json
def get_dir() -> None :
    """
    1. Get Directory
        - When the user updates their prefered folder, i need to store it to the "dir.json" file for the next load of the app.
    """
    dir = filedialog.askdirectory()
    path = os.path.join("C:\\", "vtemp", "dir.json")
    if dir :
        directory_var.set(dir)
        if os.path.isfile(path) :
            with open(os.path.join("C:\\", "vtemp", "dir.json"), "w") as json_f :
                json_f.write("")
            with open(os.path.join("C:\\", "vtemp", "dir.json"), "w") as json_f :
                json.dump({"dir" : dir}, json_f)

# gets url and pipes yt-dlp to download video over internet
def download_video() -> None :
    """
    1. Get/Set variables
        - Get the url and allocated directory for download.
        - Create an error string and checking variables for the url and dir.
    2. Check Contents
        - Check contents of variables, act as needed according to the url and directory content.
    3. Error Catching
    4. Download Video
        - Create yt-dlp options, and create the process.
        - Video will download and if their is an error, it will be raised.
        - Due to the conversion to ".mp4", sometimes it can take very long. But downloads are generally quick.
    """
    url = url_entry.get() ; directory = directory_var.get() ; Err : str = "" ; c_url : bool = False ; c_dir : bool = False
    if (not url) : c_url = bool(True)
    if (not directory) : c_dir = bool(True)
    match (c_url, c_dir) :
        case (True, True) : Err = "Please enter a valid link and select a folder."
        case (False, False) : Err = ""
        case (True, False) : Err = "Please enter a valid link."
        case (False, True) : Err = "Please select a folder."   
    if Err != "" :
        messagebox.showerror("Error", Err) ; return
    status_label.configure(text = "Please Wait... If your video is large, this will take a long time.")
    status_label.update_idletasks()
    ydl_opts = {'format': 'bestvideo[height<=1080]+bestaudio/best[height<=1080]', 'outtmpl': os.path.join(directory, '%(title)s.mp4'), 'postprocessors': [{'key': 'FFmpegVideoConvertor', 'preferedformat': 'mp4', }], }
    try :
        with yt_dlp.YoutubeDL(ydl_opts) as ydl :
            ydl.download([url])
            status_label.configure(text = "")
            status_label.update_idletasks()
        messagebox.showinfo("Success", "Video downloaded successfully!")
    except Exception as E :
        messagebox.showerror("Error", f"An error occurred: {str(E)}")
        status_label.configure(text = "")
        status_label.update_idletasks()

# root init and some other stuff
r = tk.Tk() ; r.title("Video Downloader") ; r.geometry("400x260") ; r.configure(bg = "#181818") ; directory_var = tk.StringVar()

# test for dep and env vars missing for app to work
start_test()

# objects
# Object init ; object packing ; object configuration
url_label = tk.Label(r, text = "Video URL:") ; url_label.pack(pady = 5) ; url_entry = tk.Entry(r, width = 50) ; url_entry.pack(pady = 5)
url_label.configure(fg = "#FFFFFF", bg = "#181818") ; url_entry.configure(fg = "#FFFFFF", bg = "#282828")
directory_button = tk.Button(r, text = "Select Folder", command = lambda : get_dir()) ; directory_button.pack(pady = 20)
directory_button.configure(fg = "#FFFFFF", bg = "#282828")
directory_label = tk.Label(r, textvariable = directory_var) ; directory_label.pack(pady = 5)
directory_label.configure(fg = "#FFFFFF", bg = "#181818")
download_button = tk.Button(r, text = "Download Video", command = lambda : download_video(), width = 42, height = 2) ; download_button.pack(pady = 20)
download_button.configure(fg = "#FFFFFF", bg = "#282828")
status_label = tk.Label(r, text = "") ; status_label.pack()
status_label.configure(fg = "#ce0000", bg = "#181818")

# root mainloop
r.mainloop()