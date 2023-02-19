using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading;
using System.ComponentModel;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using Path = System.IO.Path;
using MediaDevices;

namespace PlaylistsMadeEasy
{
    #region PlaylistManager
    public class PlaylistManager : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Initialized Members
        #region Constants
        public static string _DEVICENAME_TEXTBOX_STARTINGTEXT = "Enter the Friendly Name of the Device";
        public static string _MUSIC_TEXTBOX_STARTINGTEXT = "Enter the Path to the Music";
        public static string _PLAYLIST_TEXTBOX_STARTINGTEXT = "Enter the Path to the Playlist(s)";
        public static string _ERROR_PLAYLIST_PATH = "Error Getting Playlists Path";
        public static string _ERROR_MUSIC_PATH = "Error Getting Music Path";
        #endregion
        #region Fields
        //todo: need to find a way to delete the toDelete Files on phone?
        /// <summary>
        /// List of pathes to renamed playlists on phone that need to be deleted.
        /// </summary>
        private readonly List<string> toDelete = new List<string>();
        /// <summary>
        /// a list of files to copy for the selected playlists
        /// </summary>
        public List<string> FilesToCopy = new List<string>();
        /// <summary>
        /// The type playlist you want to end up with (e.g. .m3u, .wpl, etc.)
        /// </summary>
        public TargetPlaylistTypesEnum TargetPlaylistType;
        /// <summary>
        /// The type playlist you are converting from (e.g. .m3u, .wpl, etc.)
        /// </summary>
        public SourcePlaylistTypesEnum SourcePlaylistType;
        /// <summary>
        /// A temprorary directory in '%ApplicationData%/Temp/Playlists' for storing playlists before transfer
        /// </summary>
        public static string TempDirectoryCreation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Temp\Playlists");
        /// <summary>
        /// A temprorary directory in '%ApplicationData%/Temp/Playlists/FromPhoneForComparison' for storing playlists from the phone for comparison purposes (in order to obtain the difference between the phone and computer playlist versions)
        /// </summary>
        public static string TempComparisonDirectory = Path.Combine(TempDirectoryCreation, @"FromPhoneForComparison");
        internal static string preferenceFilePath = @"Options.txt";
        internal bool Converting_Complete_Flag = false;
        internal bool Copying_Complete_Flag = false;
        internal bool Getting_Difference_Complete_Flage = false;
        internal bool Copying_Started_Flag = false;
        internal bool Transfer_Started_Flag = false;
        internal int transferProgressLineCount = 1;
        internal bool DeviceConnected = false;
        internal bool DirectoryToPlaylistsHasFiles = false;
        private string RememberPreferencesString = "Remember";
        #endregion
        #region Public Fields
        /// <summary>
        /// Whether to ask before deleting any playlist at tempPlaylistPath when converting a playlist
        /// </summary>
        public bool ConfirmDelete = true;
        /// <summary>
        /// Whether to delete the target type playlist with the same name in tempPlaylistPath.  False here means any playlist in tempPlaylistPath with the same filename +  specified target type extension will be appended to rather than 
        /// </summary>
        public bool DeleteBeforeWriting = true;
        #endregion
        #region Public Bound Props   

        #region Updating UI
        #region CurrentFileBeingCopied
        private string _CurrentFileBeingCopied = "N/A";
        /// <summary>
        /// The current file name
        /// </summary>
        public string CurrentFileBeingCopied
        {
            get { return _CurrentFileBeingCopied; }
            set
            {
                if (_CurrentFileBeingCopied != value)
                {
                    _CurrentFileBeingCopied = value;
                    NotifyPropertyChanged("CurrentFileBeingCopied");
                }
            }
        }
        #endregion
        #region CurrentPlaylistBeingCopied
        private string _CurrentPlaylistBeingCopied = "N/A";
        /// <summary>
        /// Current Playlist Being Copied
        /// </summary>
        public string CurrentPlaylistBeingCopied
        {
            get { return _CurrentPlaylistBeingCopied; }
            set
            {
                if (_CurrentPlaylistBeingCopied != value)
                {
                    _CurrentPlaylistBeingCopied = value;
                    NotifyPropertyChanged("CurrentPlaylistBeingCopied");
                }
            }
        }
        #endregion
        #region TotalFilesToCopyForThisPlaylist
        private string _TotalFilesToCopyForThisPlaylist = "0";
        /// <summary>
        /// Total # of Songs in this playlist
        /// </summary>
        public string TotalFilesToCopyForThisPlaylist
        {
            get { return _TotalFilesToCopyForThisPlaylist; }
            set
            {
                if (_TotalFilesToCopyForThisPlaylist != value)
                {
                    _TotalFilesToCopyForThisPlaylist = value;
                    NotifyPropertyChanged("TotalFilesToCopyForThisPlaylist");
                }
            }
        }
        #endregion
        #region CurrentNumberOfFileBeingCopied
        private string _CurrentNumberOfFileBeingCopied = "0";
        /// <summary>
        /// The current file number (x in 'x/total')
        /// </summary>
        public string CurrentNumberOfFileBeingCopied
        {
            get { return _CurrentNumberOfFileBeingCopied; }
            set
            {
                if (_CurrentNumberOfFileBeingCopied != value)
                {
                    _CurrentNumberOfFileBeingCopied = value;
                    NotifyPropertyChanged("CurrentNumberOfFileBeingCopied");
                }
            }
        }
        #endregion
        #region ProgressBar_Value
        private int _ProgressBar_Value;
        public int ProgressBar_Value
        {
            get
            {
                return _ProgressBar_Value;
            }
            set
            {
                if (_ProgressBar_Value != value)
                {
                    _ProgressBar_Value = value;
                    NotifyPropertyChanged("ProgressBar_Value");
                }
            }
        }
        #endregion
        #region PlaylistOneName
        private string _PlaylistOneName;
        public string PlaylistOneName
        {
            get { return _PlaylistOneName; }
            set
            {
                if (_PlaylistOneName != value)
                {
                    _PlaylistOneName = value;
                    NotifyPropertyChanged("PlaylistOneName");
                }
            }
        }
        #endregion
        #region PlaylistTwoName
        private string _PlaylistTwoName;
        public string PlaylistTwoName
        {
            get { return _PlaylistTwoName; }
            set
            {
                if (_PlaylistTwoName != value)
                {
                    _PlaylistTwoName = value;
                    NotifyPropertyChanged("PlaylistTwoName");
                }
            }
        }
        #endregion
        #endregion
        #region Flags
        #region MenuItem_RememberPathes_Flag
        private bool _MenuItem_RememberPathes_Flag = false;
        public bool MenuItem_RememberPathes_Flag
        {
            get { return _MenuItem_RememberPathes_Flag; }
            set
            {
                if (_MenuItem_RememberPathes_Flag != value)
                {
                    _MenuItem_RememberPathes_Flag = value;
                    NotifyPropertyChanged("MenuItem_RememberPathes_Flag");
                }
            }
        }
        #endregion
        #endregion
        public ObservableCollection<string> PlaylistOneLinesUnique = new ObservableCollection<string>();
        public ObservableCollection<string> PlaylistTwoLinesUnique = new ObservableCollection<string>();
        public ObservableCollection<Playlist> lvPlaylistsList = new ObservableCollection<Playlist>();
        public ObservableCollection<Song> lvSongsList = new ObservableCollection<Song>();
        private List<Playlist> _Playlists = new List<Playlist>();
        /// <summary>
        /// a list of Playlist objects corresponding to the playlists found in PlaylistPathes.
        /// </summary>
        public List<Playlist> Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists != value)
                {
                    _Playlists = value;
                    NotifyPropertyChanged("Playlists");
                }
            }
        }
        private string _DeviceName = _DEVICENAME_TEXTBOX_STARTINGTEXT;
        /// <summary>
        /// The friendly name of the the phone (name displayed in Window's Explorer)
        /// </summary>
        public string DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (_DeviceName != value)
                {
                    _DeviceName = value;
                    NotifyPropertyChanged("DeviceName");
                }
            }
        }
        private string _MusicDirectory = _MUSIC_TEXTBOX_STARTINGTEXT;
        /// <summary>
        /// Directory where the music is stored
        /// </summary>
        public string MusicDirectory
        {
            get { return _MusicDirectory; }
            set
            {
                if (_MusicDirectory != value)
                {
                    _MusicDirectory = value;
                    NotifyPropertyChanged("MusicDirectory");
                }
            }
        }
        private DestinationOnPhoneLocationsEnum _DestinationOnPhone = DestinationOnPhoneLocationsEnum.Phone;
        /// <summary>
        /// Where to put files (either "Phone/Music..." or "Card/Music"
        /// </summary>
        public DestinationOnPhoneLocationsEnum DestinationOnPhone
        {
            get { return _DestinationOnPhone; }
            set
            {
                if (_DestinationOnPhone != value)
                {
                    _DestinationOnPhone = value;
                    NotifyPropertyChanged("DestinationOnPhone");
                }
            }
        }
        private string _DirectoryToPlaylists = _PLAYLIST_TEXTBOX_STARTINGTEXT;
        /// <summary>
        /// the Directory to the playlist files
        /// </summary>
        public string DirectoryToPlaylists
        {
            get { return _DirectoryToPlaylists; }
            set
            {
                if (_DirectoryToPlaylists != value)
                {
                    _DirectoryToPlaylists = value;
                    NotifyPropertyChanged("DirectoryToPlaylists");
                }
            }
        }
        private List<string> _PlaylistPathes = new List<string>();
        /// <summary>
        /// A list of strings representing the pathes of all the playlists found at DirectoryToPlaylists path
        /// </summary>
        public List<string> PlaylistPathes
        {
            get { return _PlaylistPathes; }
            set
            {
                if (_PlaylistPathes != value)
                {
                    _PlaylistPathes = value;
                    NotifyPropertyChanged("PlaylistPathes");
                }
            }
        }
        private string _TransferProgress = "";
        public string TransferProgress

        {
            get { return _TransferProgress; }
            set
            {
                if (_TransferProgress != value)
                {
                    _TransferProgress = value;
                    NotifyPropertyChanged("TransferProgress");
                }
            }
        }

        private bool Error_Displayed = false;
        #region MenuItem_ForceCopy_Flag
        private bool _MenuItem_ForceCopy_Flag = false;
        public bool MenuItem_ForceCopy_Flag
        {
            get { return _MenuItem_ForceCopy_Flag; }
            set
            {
                if (_MenuItem_ForceCopy_Flag != value)
                {
                    _MenuItem_ForceCopy_Flag = value;
                    NotifyPropertyChanged("MenuItem_ForceCopy_Flag");
                }
            }
        }
        #endregion
        #endregion
        #region Enums and Related String Arrays (For options)

        //todo: need to add to this region if adding more source playlist types
        #region Supported Source Types
        /// <summary>
        /// The supported SourcePlaylistTypes file extensions
        /// </summary>
        public readonly static string[] SourcePlaylistTypes = { @".wpl", @".m3u" };

        /// <summary>
        /// The supported SourcePlaylistTypes
        /// </summary>
        public enum SourcePlaylistTypesEnum
        {
            wpl,
            m3u
        }
        #endregion

        //todo: need to add to this region if adding more target playlist types
        #region Supported Target Types
        /// <summary>
        /// The supported TargetPlaylistType extensions
        /// </summary>
        public readonly static string[] TargetPlaylistTypes = { ".m3u", };
        /// <summary>
        /// The supported TargetPlaylistType
        /// </summary>
        public enum TargetPlaylistTypesEnum
        {
            m3u,
        }
        #endregion

        //todo: need to add to this region if adding more source playlist types
        #region Supported Source Types' Regular Expressions
        /// <summary>
        /// Regular expression used to find media pathes for the associated SourcePlaylistTypeRegularExpressionsEnum value
        /// </summary>
        public readonly static string[] SourcePlaylistTypeRegularExpressions = { @"media .+""(.+\.[a-zA-Z0-9]{3})", @"""*(\.\..+)""*" };
        /// <summary>
        /// An enum for SourcePlaylistTypeRegularExpressions
        /// </summary>
        public enum SourcePlaylistTypeRegularExpressionsEnum
        {
            wpl,
            m3u
        }
        #endregion

        #region Locations to Send Files
        /// <summary>
        /// The two locations to which files are sent either  @"/Phone/Music" or @"/Card/Music"
        /// </summary>
        public readonly static string[] DestinationOnPhoneLocations = { @"/Phone/Music", @"/Card/Music", };
        /// <summary>
        /// An enum for DestinationOnPhoneLocations
        /// </summary>
        public enum DestinationOnPhoneLocationsEnum
        {
            Phone,
            Card
        }
        #endregion

        #region Type of File Being Copied for CopyToPhone
        /// <summary>
        /// Used to add to files paths in certain methods
        /// </summary>
        private readonly static string[] TypeOfFileBeingCopied = { "", "Playlists" };
        /// <summary>
        /// An enum for TypeOfFileBeingCopied
        /// </summary>
        public enum TypeOfFileBeingCopiedEnum
        {
            File,
            Playlist
        }
        #endregion

        #endregion
        #region Needed to implement INotifyPropertyChanged (Binding to WPF)
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region Needed to implement IDataErrorInfo (WPF Tooltip)
        public string Error => null;    //returns null
        public string this[string propertyName]       //the name of the property for the current object
        {
            get
            {
                string retvalue = null;
                if (propertyName == "DirectoryToPlaylists")
                {
                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(DirectoryToPlaylists, "*" + SourcePlaylistType);
                        if (files.Length < 1)
                        {
                            DirectoryToPlaylistsHasFiles = false;
                        }
                        else
                        {
                            DirectoryToPlaylistsHasFiles = true;
                        }
                    }
                    catch { files = null; }
                    if( !Directory.Exists(DirectoryToPlaylists))
                    {
                        retvalue = $"'{DirectoryToPlaylists}' is not a valid path.  Please try a different directory.";
                    }
                    else if(DirectoryToPlaylistsHasFiles == false)
                    {
                        retvalue = $"'{DirectoryToPlaylists}' doesn't contain any playlists of type {SourcePlaylistType}.  Please try a different directory and/or source playlist type.";
                    }
                }
                else if (propertyName == "MusicDirectory")
                {
                    if (!Directory.Exists(MusicDirectory))
                    {
                        retvalue = $"'{MusicDirectory}' is not a valid path.  Please enter the directory where your music is located.";
                    }
                }
                else if (propertyName == "DeviceName")
                {                    
                    if (!DeviceConnected)
                    {
                        retvalue = $"'{DeviceName}' is not currently connected.  Please connect '{DeviceName}' and unlock it.";
                    }
                }
                return retvalue;
            }
        }
        #endregion
        #region Constructor
        //public PlaylistManager(string directoryToPlaylists, SourcePlaylistTypesEnum sourcePlaylistType = SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum targetPlaylistType = TargetPlaylistTypesEnum.m3u)
        //{
        //    #region Getting Playlist Pathes from directoryToPlaylists
        //    if (Directory.Exists(directoryToPlaylists))
        //    {
        //        string[] playlistPathes = Directory.GetFiles(directoryToPlaylists, "*" + sourcePlaylistType);
        //        foreach (string playlistPath in playlistPathes)
        //        {
        //            PlaylistPathes.Add(playlistPath);
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("directoryToPlaylists must be a string path to an existing directory containing at least one playlist of type sourcePlaylistType.  Try again. ");
        //    }
        //    #endregion

        //    DirectoryToPlaylists = directoryToPlaylists;
        //    SourcePlaylistType = sourcePlaylistType;
        //    TargetPlaylistType = targetPlaylistType;
        //}        
        /// <summary>
        /// Starts a loop that checks for Device connection via MediaDevices
        /// </summary>
        public PlaylistManager()
        {
            Task CheckMTPConnection = Task.Factory.StartNew(() =>
            {
                while (!Transfer_Started_Flag)
                {
                    GetDeviceConnected();
                    Thread.Sleep(500);
                }
            });

        }
        #endregion
        #endregion
        #region Public Methods
        #region StartTransfer (The Main Sequence)
        /// <summary>
        /// Runs ConvertPlaylists and copies files and playlists to phone
        /// </summary>
        public void StartTransfer(List<string> playlistsToTransfer)
        {
            Error_Displayed = false;
            TransferProgress = "";
            UpdateTransferProgress("Transfer Started", true);
            Transfer_Started_Flag = true;
            List<Playlist> playlistsWithSongsToCopy = new List<Playlist>();
            #region Converting Playlists
            Task ConvertPlaylistsTask = Task.Factory.StartNew(() =>
            {
                //PlaylistPathes used in ConvertPlaylists
                PlaylistPathes = playlistsToTransfer;
                ConvertPlaylists();
                UpdateTransferProgress("Converting Playlists Complete");
                //MessageBox.Show(string.Format("Converting Playlists Complete"));
                Converting_Complete_Flag = true;
            });
            #endregion
            #region GetDifferenceBetweenPlaylists()

            Task playlistsWithSongsToCopyTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    playlistsWithSongsToCopy = GetDifferenceBetweenPlaylists(playlistsToTransfer);
                    UpdateTransferProgress("Found Difference Between Playlists Complete");
                }
                catch (Exception)
                {
                    MessageBox.Show($"Check the spelling of '{DeviceName}' as it must match exactly as shown in Windows' Explorer.  If the spelling is correct but it is not showing up in Window's Explorer, re-connect the device and unlock it.  If this doesn't work, try using a different USB port and/or USB cable.\n.\nPlease re-select any playlists you want to transfer after re-connecting and unlocking your device.'", $"Unable to Find {DeviceName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Error_Displayed = true;

                    //Delete TempComparisonDirectory on Exception
                    Directory.Delete(TempComparisonDirectory, true);
                }
            });
            #endregion

            #region Copying Files and Playlists
            Task CopyFilesTask = Task.Factory.StartNew(() =>
            {
                ConvertPlaylistsTask.Wait();
                playlistsWithSongsToCopyTask.Wait();
                CopyToPhone(playlistsWithSongsToCopy, DeviceName, DestinationOnPhoneLocationsEnum.Card, false);

                if (!Error_Displayed)
                {
                    Copying_Complete_Flag = true;
                    string msg = "Transfer Complete";
                    UpdateTransferProgress(msg);
                    CurrentFileBeingCopied = string.Format(msg);
                }
                else
                {
                    Copying_Complete_Flag = false;
                    string msg = "Error during Transfer.  Please try again.";
                    UpdateTransferProgress(msg);
                    CurrentFileBeingCopied = string.Format(msg);
                }
                ResetMembers();
            });
            #endregion
        }
        #endregion
        #region ConvertPlaylists
        /// <summary>
        /// Creates a List of source Playlist objects for each of the playlists found in PlaylistPathes.  Stored in 'Playlists' property
        /// </summary>
        public void ConvertPlaylists()
        {
            //Creates a Temporary Playlist Directory called tempDirectory
            Directory.CreateDirectory(TempDirectoryCreation);

            #region Parallel Creation of Playlists in a temp directory

            #region Creating Cancellation Token and setting MaxDegreeOfParallelism
            CancellationTokenSource cts = new CancellationTokenSource();
            ParallelOptions parallelOptions = new ParallelOptions
            {
                //Use parallelOptions instance to store the CancellationToken
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = System.Environment.ProcessorCount
            };
            #endregion

            #region Looping through Playlists to Create New Ones
            Parallel.ForEach(PlaylistPathes, parallelOptions, (playlist, state) =>
            {
                string[] playlistContents = File.ReadAllLines(playlist);
                string tempPlaylistPath = Path.Combine(TempDirectoryCreation, Path.GetFileNameWithoutExtension(playlist) + TargetPlaylistTypes[(int)TargetPlaylistType]);
                CreateNewPlaylist(playlistContents, tempPlaylistPath, SourcePlaylistType, TargetPlaylistType);
            });
            #endregion

            #endregion
        }
        #endregion
        #region GetDifferenceBetweenPlaylists
        /// <summary>
        /// Copies the playlists in playlistPathesOnPhone from phone to a temporary folder on computer and then calls GetSongsToCopy() to compare them to the source playlists found in the object property PlaylistPathes.  Returns a list of strings representing pathes to files to copy to the phone based on the difference between the phone and local playlist of the same name.
        /// </summary>
        /// <param name="playlistPathesOnPhone">List strings representing pathes to playlists on the phone to be transferred to the computer and compared to playlis of the same name on the computer</param>
        /// <param name="playlistTypeOnComputer">The file format of the source playlist on computer</param>
        /// <param name="playlistTypeOnPhone">The file format of the target playlist on phone</param>
        /// <returns>Returns a list of strings representing files to copy to the phone based on the difference between the phone and local playlist of the same name</returns>
        internal List<Playlist> GetDifferenceBetweenPlaylists(List<String> playlistsToCopy, SourcePlaylistTypesEnum playlistTypeOnComputer = SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum playlistTypeOnPhone = TargetPlaylistTypesEnum.m3u)
        {
            #region Initialization
            List<Playlist> playlistsFromPhone = new List<Playlist>();
            List<Playlist> playlistsFromComputer = new List<Playlist>();
            List<string> playlistPathesOnPhone = GetTargetPlaylists(playlistsToCopy);
            string directoryLocalOfPhonePlaylists = null;
            string directoryLocalOfComputerPlaylists = Path.Combine(MusicDirectory, "temp");
            string filenameOnPhone;
            string filenameOnComputer;
            #endregion
            #region Deleting TempComparisonDirectory if it exists
            if (Directory.Exists(TempComparisonDirectory))
            {
                Directory.Delete(TempComparisonDirectory, true);
            }
            Directory.CreateDirectory(TempComparisonDirectory);
            #endregion
            #region Transfer the playlists in question from Phone to TempComparingDirectory
            var devices = MediaDevice.GetDevices();

            using (var device = devices.First(d => d.FriendlyName == DeviceName))
            {
                device.Connect();
                foreach (string playlist in playlistPathesOnPhone)
                {
                    string playlistOnPhone = playlist + TargetPlaylistTypes[(int)playlistTypeOnPhone];
                    if (device.FileExists(playlistOnPhone))
                    {
                        #region Getting FileName and Directory on Phone
                        string filenameWithoutExtension = Path.Combine(Path.GetFileNameWithoutExtension(playlistOnPhone));
                        filenameOnPhone = filenameWithoutExtension + TargetPlaylistTypes[(int)playlistTypeOnPhone];
                        directoryLocalOfPhonePlaylists = Path.Combine(TempComparisonDirectory, filenameOnPhone);
                        #endregion

                        #region Getting FileName and Directory on Computer
                        filenameOnComputer = filenameWithoutExtension + Regex.Replace(SourcePlaylistTypes[(int)playlistTypeOnComputer], @"\*", @"");
                        directoryLocalOfComputerPlaylists = Path.Combine(DirectoryToPlaylists, filenameOnComputer);
                        #endregion

                        device.DownloadFile(playlistOnPhone, directoryLocalOfPhonePlaylists);
                    }
                }
                device.Disconnect();
            }
            #endregion
            #region Get the Contents of all the Playlists in the Local Temp Folder
            string[] playlistPathes = Directory.GetFiles(TempComparisonDirectory);
            if (playlistPathes.Length > 0)
            {
                foreach (string playlistPath in playlistPathes)
                {
                    //todo: need to add more if statements for conversions here if adding new target types
                    SourcePlaylistTypesEnum newPlaylistTypeOnPhone = SourcePlaylistTypesEnum.m3u;
                    if (playlistTypeOnPhone == TargetPlaylistTypesEnum.m3u)
                    {
                        newPlaylistTypeOnPhone = SourcePlaylistTypesEnum.m3u;
                    }
                    playlistsFromPhone.AddRange(GetPlaylists(playlistPath, newPlaylistTypeOnPhone));
                }
            }
            else
            {
                playlistsFromPhone = null;
            }
            #endregion
            #region Get the Corresponding playlist path from PlaylistPathes (Created at Initialization)
            foreach (string playlist in playlistsToCopy)
            {
                playlistsFromComputer.AddRange(GetPlaylists(playlist, playlistTypeOnComputer));
            }


            #endregion
            #region Testing playlistsFromPhone
            //foreach (var p in playlistsFromPhone)
            //{
            //    MessageBox.Show(string.Format("Testing playlistsFromPhone: {0}\n p.Songs.Count: {1}", p.Name, p.Songs.Count));
            //    foreach (var song in p.Songs)
            //    {
            //        MessageBox.Show(string.Format("Song Name: {0}'s path: {1}", song.Name, song.Path));
            //    }
            //}
            #endregion
            #region Testing playlistsFromComputer
            //foreach (var p2 in playlistsFromComputer)
            //{
            //    MessageBox.Show(string.Format("Testing playlistsFromComputer: {0}\n p2.Songs.Count: {1}", p2.Name, p2.Songs.Count));
            //    foreach (var song in p2.Songs)
            //    {
            //        MessageBox.Show(string.Format("Song Name: {0}'s path: {1}", song.Name, song.Path));
            //    }
            //}
            #endregion
            return GetPlaylistsWithSongsToCopy(playlistsFromComputer, playlistsFromPhone);
        }
        #endregion
        #region GetPlaylistsWithSongsToCopy
        /// <summary>
        /// Gets the difference between playlistsFromComputer and playlistsFromPhone for each Playlist.  Returns a Queue string of songs that need to be copied to GetDifferenceBetweenPlaylists().
        /// </summary>
        /// <param name="playlistsFromComputer">List of Playlist objects from computer to check</param>
        /// <param name="playlistsFromPhone">List of Playlist objects from phone to check</param>
        /// <returns> Returns to GetDifferenceBetweenPlaylists() a List of strings representing pathes to media files that need to be copied to the phone .</returns>
        public List<Playlist> GetPlaylistsWithSongsToCopy(List<Playlist> playlistsFromComputer, List<Playlist> playlistsFromPhone)
        {
            List<Playlist> playlistsWithSongsToCopy = new List<Playlist>();
            HashSet<string> songsToCopy;
            string playlistName = null;
            string sourcePlaylistPath = null;
            #region Compare the playlists to Populate the songsToCopy Queue
            if (playlistsFromPhone != null)
            {
                #region Getting the songsToCopy for the Playlist
                for (int i = 0; i < playlistsFromComputer.Count; i++)
                {
                    songsToCopy = new HashSet<string>();
                    #region Get Details of Playlist (Name and Path) to Add to Result after for loop.  Also set playlistInTempComparison
                    sourcePlaylistPath = playlistsFromComputer[i].Path;
                    playlistName = playlistsFromComputer[i].Name;
                    string playlistInTempComparison = Path.Combine(TempComparisonDirectory, playlistsFromComputer[i].Name + TargetPlaylistTypes[(int)TargetPlaylistType]);
                    bool copyCurrentPlaylist = false;
                    #endregion
                    #region Case: Playlist Exists in playlistInTempComparison (on Phone)
                    if (File.Exists(playlistInTempComparison))
                    {
                        #region Getting the Playlist to Iterate OVer and to compare against for next Loop and Handling Same Playlist Scenario
                        //MessageBox.Show(string.Format("playlistsFromPhone.Name: {0}\nplaylistsFromPhone.Songs.Count: {1}\nplaylistsFromComputer.Name: {2}\nplaylistsFromComputer.Songs.Count: {3}", targetPlaylist[0].Name, targetPlaylist[0].Songs.Count, playlistsFromComputer[i].Name, playlistsFromComputer[i].Songs.Count));
                        Playlist playlistOnComputer = new Playlist();
                        Playlist playlistOnPhone = new Playlist();
                        string targetPlaylistPath = Path.Combine(TempComparisonDirectory, playlistName + TargetPlaylistTypes[(int)TargetPlaylistType]);
                        ObservableCollection<Playlist> targetPlaylist = GetPlaylists(targetPlaylistPath, SourcePlaylistTypesEnum.m3u);

                        #region Moving on to the Next Playlist if the Hashes Match
                        if (GetHash(sourcePlaylistPath) == GetHash(targetPlaylistPath))
                        {
                            string msg = $"Skipping {playlistsFromComputer[i].Name} as they are the same.";
                            UpdateTransferProgress(msg);
                            //MessageBox.Show(string.Format(msg));
                            continue;
                        }
                        #endregion

                        //#region Setting playlistOnComputer and playlistOnPhone
                        //if (playlistsFromComputer[i].Songs.Count > targetPlaylist[0].Songs.Count)
                        //{
                        //    playlistOnComputer = playlistsFromComputer[i];
                        //    playlistOnPhone = targetPlaylist[0];
                        //}
                        //else if (playlistsFromComputer[i].Songs.Count < targetPlaylist[0].Songs.Count)
                        //{
                        //    playlistOnComputer = targetPlaylist[0];
                        //    playlistOnPhone = playlistsFromComputer[i];
                        //}
                        //else
                        //{
                        //    playlistOnComputer = playlistsFromComputer[i];
                        //    playlistOnPhone = targetPlaylist[0];
                        //}
                        //#endregion

                        #endregion
                        #region Getting the difference between the two playlists
                        playlistOnComputer = playlistsFromComputer[i];
                        playlistOnPhone = targetPlaylist[0];
                        int countBeforeSetSubtraction = playlistOnComputer.SongNames.Count;
                        playlistOnComputer.SongNames.ExceptWith(playlistOnPhone.SongNames);
                        copyCurrentPlaylist = true;

                        if (playlistOnComputer.SongNames.Count > 0)
                        {
                            HashSet<string> uniqueSongNames = playlistOnComputer.SongNames;
                            int countAfterSetSubtraction = playlistOnComputer.SongNames.Count;
                            //MessageBox.Show(string.Format("SongNames Before: {0}", countBeforeSetSubtraction));
                            //MessageBox.Show(string.Format("SongNames After: {0}", countAfterSetSubtraction));
                            #endregion
                            #region Case where playlists have the same number of songs
                            List<string> playlistWithMoreSongsSongNames = playlistOnComputer.SongNames.ToList();
                            List<string> playlistWithFewerSongsSongNames = playlistOnPhone.SongNames.ToList();
                            if (countAfterSetSubtraction == countBeforeSetSubtraction)
                            {
                                //MessageBox.Show(string.Format("1: {0}", 1));
                                foreach (Song song in playlistOnComputer.Songs)
                                {
                                    bool addToResult = true;
                                    foreach (Song song2 in playlistOnPhone.Songs)
                                    {
                                        //MessageBox.Show(string.Format("song: {0}\n song2: {1}", song, song2));
                                        if (song.Name.ToUpper() == song2.Name.ToUpper())
                                        {
                                            addToResult = false;
                                            break;
                                        }
                                    }
                                    if (addToResult)
                                    {
                                        //MessageBox.Show(string.Format("Adding Song: {0}", song.Path));
                                        songsToCopy.Add(song.Path);
                                        copyCurrentPlaylist = true;
                                    }
                                }
                            }
                            #endregion
                            #region If the playlists have a different number of songs
                            else
                            {
                                copyCurrentPlaylist = true;
                                foreach (string songName in uniqueSongNames)
                                {
                                    foreach (Song song in playlistOnComputer.Songs)
                                    {
                                        if (songName.ToUpper() == song.Name.ToUpper())
                                        {
                                            //MessageBox.Show(string.Format("Adding Song: {0}", song.Path));
                                            songsToCopy.Add(song.Path);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                    #region Case: Playlist only exists on Computer (not at playlistInTempComparison)
                    else
                    {
                        songsToCopy.UnionWith(GetAllSongPathes(playlistsFromComputer[i]).ToHashSet());
                        copyCurrentPlaylist = true;
                    }
                    #endregion
                    #region Creating a playlist with songsToCopy and the same path
                    if (copyCurrentPlaylist)
                    {
                        playlistsWithSongsToCopy.Add(new Playlist()
                        {
                            Name = playlistName,
                            Path = sourcePlaylistPath,
                            SongPathesToCopy = songsToCopy
                        });
                    }
                    #endregion
                }
                #endregion
            }
            #region Otherwise copying all the songs in the playlists from playlistsFromComputer as none exist on phone
            else
            {
                songsToCopy = new HashSet<string>();
                foreach (Playlist playlist in playlistsFromComputer)
                {
                    playlistsWithSongsToCopy.Add(new Playlist()
                    {
                        Name = playlist.Name,
                        Path = playlist.Path,
                        SongPathesToCopy = GetAllSongPathes(playlist).ToHashSet()
                    });
                }
            }
            #endregion

            #endregion
            return playlistsWithSongsToCopy;
        }
        #endregion
        #region CopyToPhone
        /// <summary>
        /// Copies files to DeviceName.  Keeping the original names and placing them in the destinationOnPhone directory.  Doesn't do any checking to compare files other than to see if the file exists.  Use forceCopy to force the transfer even if the file exists on phone..
        /// </summary>
        /// <param name="playlists">List of strings representing pathes to individual playlists/files to copy</param>
        /// <param name="phoneName">name of the phone as shown in Windows Explorer</param>
        /// <param name="phoneDestinationDirectory">Where on the phone you want to copy the playlist to (e.g. "\Card\Music\Playlists\PlaylistTemplate.m3u")</param>
        /// <param name="forceCopy">True forces the transfer even if the file exists on the phone.</param>
        /// 
        /// 
        public void CopyToPhone(List<Playlist> playlists, string phoneName, DestinationOnPhoneLocationsEnum phoneDestinationDirectory, bool forceCopy = true)
        {
            //todo: bug when loading songs from playlist into list box.  also add option to delete lines from playlist file?
            //MessageBox.Show(string.Format("Starting"));
            Copying_Started_Flag = true;
            #region Connecting to Device
            try
            {
                var devices = MediaDevice.GetDevices();
                var device = devices.First(d => d.FriendlyName == phoneName);

                #endregion
                int reconnectInterval = 5;
                #region Coping the Files then the Playlist if all Files Copied Sucessfully
                try
                {
                    using (device)
                    {
                        device.Connect();
                        foreach (Playlist playlist in playlists)
                        {
                            string msg = string.Format("Starting Copy of: {0}{1} Playlist", playlist.Name, TargetPlaylistTypes[(int)TargetPlaylistType]);
                            UpdateTransferProgress(msg);
                            #region Setting UI Properties
                            int songsChecked = 0;
                            int totalCount = playlist.SongPathesToCopy.Count;
                            CurrentPlaylistBeingCopied = playlist.Name;
                            #endregion
                            //todo: add condition here for option to copy playlist through options
                            
                            //if (playlist.SongPathesToCopy.Count < 1 && MenuItem_ForceCopy_Flag == false)
                            //{
                            //    copyPlaylist = false;
                            //    msg = $"Skipping '{playlist.Name}' as it is the same.";
                            //    UpdateTransferProgress(msg);
                            //    //MessageBox.Show(msg, $"Copying Not Necessary for {playlist.Name}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //}
                            //else
                            //{
                            TotalFilesToCopyForThisPlaylist = string.Format("{0}", totalCount);
                            //}
                            #region Copying the Songs in the Playlist object
                            foreach (var songToCopyPath in playlist.SongPathesToCopy)
                            {
                                #region Ui Updates
                                CurrentFileBeingCopied = Path.GetFileName(songToCopyPath);
                                CurrentNumberOfFileBeingCopied = string.Format("{0}", songsChecked + 1);
                                ProgressBar_Value = (int)Math.Round((double)(100 * (songsChecked + 1)) / totalCount);
                                #endregion

                                //MessageBox.Show(string.Format("TransferProgressList: {0}", TransferProgressList));
                                if (songsChecked % reconnectInterval == 0)
                                {
                                    device.Connect();
                                }
                                string destOnPhone = Path.Combine(DestinationOnPhoneLocations[(int)phoneDestinationDirectory], Path.GetFileName(songToCopyPath));
                                if (!device.FileExists(destOnPhone))
                                {
                                    string sourcePath = Regex.Replace(songToCopyPath, @"^\s*\.\.", MusicDirectory);
                                    bool songExistsOnLocalMachine = File.Exists(sourcePath);
                                    if (!songExistsOnLocalMachine)
                                    {
                                        MessageBox.Show(string.Format("sourcePath: {0} is non existant.  Add feature to remove from playlist?", sourcePath));
                                    }
                                    else
                                    {
                                        msg = $"Copying {sourcePath}";
                                        UpdateTransferProgress(msg);
                                        device.UploadFile(sourcePath, destOnPhone);
                                        songsChecked++;
                                    }
                                }
                                else
                                {
                                    msg = $"Skipping {songToCopyPath}";
                                    UpdateTransferProgress(msg);
                                    songsChecked++;
                                }
                                if (songsChecked % reconnectInterval == 0)
                                {
                                    device.Disconnect();
                                }
                            }

                            #region Copying the Playlist if all Songs Transferred
                            device.Connect();
                            if (songsChecked == playlist.SongPathesToCopy.Count)
                            {
                                //todo: need to remove hard-coding here of "playlists"
                                string targetPlaylistExtension = TargetPlaylistTypes[(int)TargetPlaylistType];
                                string destOnPhone = Path.Combine(DestinationOnPhoneLocations[(int)phoneDestinationDirectory], "Playlists");
                                string playlistNameWithExtension = Path.GetFileName(playlist.Name) + targetPlaylistExtension;
                                string targetPath = Path.Combine(destOnPhone, playlistNameWithExtension);
                                string sourcePath = Path.Combine(TempDirectoryCreation, playlistNameWithExtension);

                                msg = string.Format("Playlist: {0} finished", playlist.Name + targetPlaylistExtension);
                                UpdateTransferProgress(msg, true);    
                                if (device.FileExists(targetPath) && GetHash(sourcePath) != GetHash(targetPath))
                                {
                                    Guid guid = Guid.NewGuid();
                                    device.Rename(targetPath, guid.ToString());
                                }                         
                                device.UploadFile(sourcePath, targetPath);
                            }
                            #endregion
                            #endregion
                        }
                        if (Directory.Exists(TempComparisonDirectory))
                        {
                            Directory.Delete(TempComparisonDirectory, true);
                        }

                        //todo: figure out how to delete renamed file
                        //device.DeleteFile(Path.Combine(destOnPhone, guid.ToString()));
                    }
                }
                #endregion
                #region Handling Exceptions of Copying
                catch (InvalidOperationException)
                {
                    MessageBox.Show(string.Format("Unable connect to {0}.\nPlease make sure it is connected and shows up in Windows Explorer.\nAlso, check that you typed the name of the phone correctly (it is case-sensitive)", phoneName), $"Issue Connecting to '{phoneName}'.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException)
                {
                    MessageBox.Show($"Issue transferring files.  Please refrain from using {phoneName} while the transfer is in progress.", $"Issue Connecting to {phoneName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception a)
                {
                    MessageBox.Show(string.Format("Unhandled Exception:\n{0}: \n{1}", a.GetType().Name, a.Message));
                }
                finally
                {
                    device.Disconnect();
                }
                #endregion
            }
            #region Handling Device Connection Issues
            catch (Exception)
            {
                if (Error_Displayed == false)
                {
                    MessageBox.Show($"Issues connecting to '{phoneName}'.  Is it plugged in and unlocked?  Also, check the spelling as it is case-sensitive.", $"Unable to Find {phoneName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            #endregion
        }
        #endregion
        #region GetPlaylists
        /// <summary>
        /// Creates and returns a list of Playlist objects from sourcePlaylistPath of type playlistType"
        /// </summary>
        /// <param name="playlistPath">Path to a directory with playlist of playlistType in it or path to a playlist file of playlistType</param>
        /// <param name="playlistType">Type of playlist</param>
        /// <returns>a list of playlist objects found at the sourcePlaylistPath of type playlistType</returns>
        public ObservableCollection<Playlist> GetPlaylists(string playlistPath, SourcePlaylistTypesEnum playlistType = SourcePlaylistTypesEnum.wpl)
        {
            #region Initialization Stuff
            string[] playlists = new string[1];
            ObservableCollection<Playlist> returnPlaylists = new ObservableCollection<Playlist>();
            #endregion

            #region Handling File, Directory, and Neither Cases 
            //todo: the first conditional ideally would only allow supported Source Playlist Types from SourcePlaylistTypesEnum
            if (File.Exists(playlistPath))
            {
                playlists.SetValue(playlistPath, 0);
            }
            else if (Directory.Exists(playlistPath))
            {
                playlists = Directory.GetFiles(playlistPath, "*" + SourcePlaylistTypes[(int)playlistType]);
            }
            else
            {
                throw new Exception($"{playlistPath} is either not a path to a file or a directory.");
            }
            #endregion

            #region Parallel Foreach (Creating Playlist objects with embedded Songs object)

            #region Parallel Foreach Cancellation Token and Options
            CancellationTokenSource cts = new CancellationTokenSource();
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = cts.Token;
            parallelOptions.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            #endregion

            Parallel.ForEach(playlists, parallelOptions, (playlist, state) =>
            {
                #region Getting all the songPathes and creating a Song then adding Songs to songsToAdd
                string[] fileLines = File.ReadAllLines(playlist);
                HashSet<Song> songsToAdd = new HashSet<Song>();
                HashSet<string> songNamesToAdd = new HashSet<string>();
                foreach (string line in fileLines)
                {
                    string songPath = GetPathOfMedia(line, playlistType);
                    if (songPath != null)
                    {
                        #region Creating Song Objects and songNames to Add to returnPlaylists
                        //MessageBox.Show(string.Format("Path.GetFileName(songPath): {0}", Path.GetFileName(songPath)));
                        songNamesToAdd.Add(Path.GetFileName(songPath));
                        Song nextSongToAdd = new Song()
                        {
                            PlaylistName = Path.GetFileNameWithoutExtension(playlist),
                            Path = songPath,
                            Extension = Path.GetExtension(songPath),
                            Name = Path.GetFileName(songPath)
                        };
                        songsToAdd.Add(nextSongToAdd);
                        #endregion
                    }
                }
                #endregion

                #region Adding the Playlist to the Playlists List
                returnPlaylists.Add(new Playlist()
                {
                    Name = Path.GetFileNameWithoutExtension(playlist),
                    Path = playlist,
                    Type = playlistType,
                    Songs = songsToAdd,
                    SongNames = songNamesToAdd,
                });
                #endregion

                #region Testing songsToAdd
                    //foreach (Song song in songsToAdd)
                    //{
                    //    MessageBox.Show(string.Format("song.Name: {0}\n song.Path: {1}\n song.Extenision {2}\n song.Title {3}\n song.Album {4}\n song.Artists.Length {5}", song.Name, song.Path, song.Extension, song.Title, song.Album, song.Artists.Length));
                    //}
                #endregion

                #region TestLoadPlaylists
                //foreach (var playlist2 in Playlists)
                //{
                //    MessageBox.Show(string.Format("playlist2.Name: {0}\n playlist2.Type: {1}\nplaylist.Path: {2}\n playlist2.Songs.Count: {3}", playlist2.Name, playlist2.Type, playlist2.Path, playlist2.Songs.Count));
                //}
                #endregion
            });
            #endregion

            return returnPlaylists;
        }
        #endregion
        #region ResetMembers
        private void ResetMembers()
        {
            Converting_Complete_Flag = false;
            Copying_Complete_Flag = false;
            Getting_Difference_Complete_Flage = false;
            Copying_Started_Flag = false;
            Transfer_Started_Flag = false;
            Interlocked.Exchange(ref transferProgressLineCount, 1);

        }
        #endregion
        #endregion
        #region Private Methods
        #region CreateNewPlaylist
        /// <summary>
        /// Creates  a new playlist of type targetPlaylistType from playlistContents of source type sourcePlaylistType .  Calls GetPathToMedia() for the regex conversion process.
        /// </summary>
        /// <param name="playlistContents">The lines of the source playlist</param>
        /// <param name="tempPlaylistPath">The location to put the new playlist in</param>
        /// <param name="sourcePlaylistType">The source playlist type (e.g. WPL, M3u, etc)</param>
        public void CreateNewPlaylist(string[] playlistContents, string tempPlaylistPath, SourcePlaylistTypesEnum sourcePlaylistType = SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum targetPlaylistType = TargetPlaylistTypesEnum.m3u)
        {
            #region Deleting newPlaylistPath if specified
            if (DeleteBeforeWriting)
            {
                if (ConfirmDelete)
                {
                    if (MessageBox.Show(string.Format("Are you sure you want to delete {0}?", tempPlaylistPath), "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        File.Delete(tempPlaylistPath);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Cancelling Deletion of {0}?", tempPlaylistPath), "Deletion Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    if (File.Exists(tempPlaylistPath))
                    {
                        File.Delete(tempPlaylistPath);
                    }
                }
            }
            #endregion

            #region Actual Creation
            foreach (string line in playlistContents)
            {
                string songPath = null;
                //todo: need to add  different playlist source types, need to create more regions for each source type     
                #region SourceType = WPL
                if (sourcePlaylistType == SourcePlaylistTypesEnum.wpl)
                {
                    #region TargetType = M3U
                    if (targetPlaylistType == TargetPlaylistTypesEnum.m3u)
                    {
                        songPath = GetPathOfMedia(line, SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum.m3u);
                    }
                    #endregion

                    //todo: need to add  different playlist target types, need to work on logic here           
                    else
                    {
                        songPath = GetPathOfMedia(line, SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum.m3u);
                    }
                }
                #endregion

                #region Writing matches to the playlist
                if (songPath != null)
                {
                    //MessageBox.Show(string.Format("songPathOld: {0}\nsongPathNew: {1}", songPath, newSongPath));
                    if (!songPath.Trim().StartsWith(@"\.\./"))
                    {
                        songPath = string.Format("../{0}", Path.GetFileName(songPath));
                    }
                    using (StreamWriter sw = File.AppendText(tempPlaylistPath))
                    {
                        sw.WriteLine(songPath);
                        lock (FilesToCopy)
                        {
                            FilesToCopy.Add(songPath);
                        }
                    };
                }
                #endregion
            }
            #endregion
        }
        #endregion
        #region GetPathOfMedia
        /// <summary>
        /// Takes in a playlist line sourcePlaylistLine and returns it from the sourcePlaylistType to the targetPlaylistType.
        /// </summary>
        /// <param name="sourcePlaylistLine">the string to find the match in</param>
        /// <param name="sourcePlaylistType">The type of the source playlist (e.g. WPL, M3U, etc)</param>
        /// <param name="targetPlaylistType">The type of the target playlist (e.g. WPL, M3U, etc) </param>
        /// <returns>Returns a  string representing the path to the media content for the new targetPlaylistType.  Returns null if no media path found based on the SourcePlaylistTypeRegularExpressions value</returns>
        private string GetPathOfMedia(string sourcePlaylistLine, SourcePlaylistTypesEnum sourcePlaylistType = SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum targetPlaylistType = TargetPlaylistTypesEnum.m3u)
        {
            MatchCollection mediaFilePathes = Regex.Matches(sourcePlaylistLine, SourcePlaylistTypeRegularExpressions[(int)sourcePlaylistType]);
            if (mediaFilePathes.Count > 0)
            {
                string res = null;
                //todo: need to add more source and target types, add the required regions here
                #region TargetType = M3U
                if (targetPlaylistType == TargetPlaylistTypesEnum.m3u)
                {
                    #region SourceType = WPL
                    if (sourcePlaylistType == SourcePlaylistTypesEnum.wpl)
                    {
                        #region Replacing Illegal Character in target type
                        string[] problemStrings = { @"&amp;", @"&apos;", @"\\", @"\.fla" };
                        string[] remedyStrings = { @"&", @"'", @"/", @".flac" };
                        foreach (Match match in mediaFilePathes)
                        {
                            res = match.Groups[1].ToString().Trim();
                            for (int i = 0; i < problemStrings.Length; i++)
                            {
                                res = Regex.Replace(res, problemStrings[i], remedyStrings[i]);
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region SourceType = M3U
                    if (sourcePlaylistType == SourcePlaylistTypesEnum.m3u)
                    {
                        foreach (Match match in mediaFilePathes)
                        {
                            res = match.Groups[1].ToString().Trim();
                        }
                    }
                    #endregion
                }
                #endregion

                return res;
            }
            return null;
        }
        #endregion
        #region GetAllSongPathes
        /// <summary>
        /// Gets all the pathes to songs for the input playlist object
        /// </summary>
        /// <param name="playlist">the playlist with the songs to get the pathes for</param>
        /// <returns>A List of strings representing pathes to songs in playlist</returns>
        private List<string> GetAllSongPathes(Playlist playlist)
        {
            List<string> songsToCopy = new List<string>();
            foreach (Song song in playlist)
            {
                songsToCopy.Add(song.Path);
            }
            return songsToCopy;
        }
        #endregion
        #endregion
        #region Helper Methods
        #region GetTargetPlaylists
        /// <summary>
        /// Takes a list of local playlist pathes and converts them to phone playlist pathes.  Precedes GetDifferenceBetweenPlaylists and provides input for it.
        /// </summary>
        /// <param name="nextPlaylists">A List of strings representing the local playlists to have their local pathes converted to phone pathes (e.g. "c:\users\adam\playlists" to "/Card/Music/Playlists"</param>
        /// <returns>A List of strings representing the location on the phone where the playlist will end up </returns>
        public List<string> GetTargetPlaylists(List<string> nextPlaylists, DestinationOnPhoneLocationsEnum destinationOnPhone = DestinationOnPhoneLocationsEnum.Card)
        {
            List<string> targetPlaylists = new List<string>();
            foreach (string nextPlaylist in nextPlaylists)
            {
                string playlistLocation = Path.Combine(PlaylistManager.DestinationOnPhoneLocations[(int)destinationOnPhone], "Playlists");
                string playlistName = Path.GetFileNameWithoutExtension(nextPlaylist);
                string targetPlaylist = Path.Combine(playlistLocation, playlistName);
                targetPlaylists.Add(targetPlaylist);
            }
            return targetPlaylists;
        }
        #endregion
        #region GetHash(string pathToFile)
        /// <summary>
        /// Calculates the SHA256 hash of the file at pathToFile
        /// </summary>
        /// <param name="pathToFile">string path to a file</param>
        /// <returns>a string representing the SHA256 hash of a file</returns>
        public static string GetHash(string pathToFile)
        {
            // The cryptographic service provider.
            SHA256 Sha256 = SHA256.Create();
            byte[] res;
            // Compute the file's hash.
            using (FileStream stream = File.OpenRead(pathToFile))
            {
                res = (Sha256.ComputeHash(stream));
            }
            return BytesToString(res);
        }

        #region BytesToString 
        /// <summary>
        /// Returns a string represnting a byte array as a sequence of hex values
        /// </summary>
        /// <param name="bytes">the byte array to convert tohex</param>
        /// <returns>Returns a string represnting a byte array as a sequence of hex values</returns>
        private static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        #endregion

        #endregion
        #region DirectoryCopy
        /// <summary>
        /// Copies a directory tree from sourceDirName to destDirName.  True for copySubDirs is necessary for dirs with files and su
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!File.Exists(temppath))
                {
                    file.CopyTo(temppath, false);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        #endregion
        #region WritePreferencesToFiles
        /// <summary>
        /// Write the previous MusicPath, DeviceName, and DirectoryToPlaylists to a file called options.txt in the same dir as the .exe.
        /// </summary>
        public void WritePreferencesToFiles(string lastPlaylistPath, string lastMusicPath, string lastDeviceName)
        {
            StringBuilder textToWrite = new StringBuilder();
            string loadPathesLine = $"{RememberPreferencesString} = {(MenuItem_RememberPathes_Flag == true ? "1" : "0")}";
            textToWrite.AppendLine(loadPathesLine);
            textToWrite.AppendLine("#This is the Last Path to Playlists");
            textToWrite.AppendLine(string.Format("{0}, {1}", "DirectoryToPlaylists", MenuItem_RememberPathes_Flag == true ? lastPlaylistPath : ""));
            textToWrite.AppendLine("\n");
            textToWrite.AppendLine("#This is the Last Path to Music");
            textToWrite.AppendLine(string.Format("{0}, {1}", "MusicDirectory", MenuItem_RememberPathes_Flag == true ? lastMusicPath : ""));
            textToWrite.AppendLine("\n");
            textToWrite.AppendLine("#This is the Last Device Name");
            textToWrite.AppendLine(string.Format("{0}, {1}", "DeviceName", MenuItem_RememberPathes_Flag == true ? lastDeviceName : ""));
            File.WriteAllText(preferenceFilePath, textToWrite.ToString());
        }
        #endregion
        #region GetPreferencesFromFile
        /// <summary>
        /// Sets DirectoryToPlaylists, MusicDirectory, and DeviceName from preferenceFilePath
        /// </summary>
        public void GetPreferencesFromFile()
        {
            string options = null;
            if (File.Exists(preferenceFilePath))
            {
                options = File.ReadAllText(preferenceFilePath);
                if (Regex.Match(options, $"{RememberPreferencesString}\\s*=\\s*1", RegexOptions.IgnoreCase).Success)
                {
                    MenuItem_RememberPathes_Flag = true;
                    DirectoryToPlaylists = Regex.Match(options, @"DirectoryToPlaylists, (.+)").Groups[1].Value.Trim();
                    MusicDirectory = Regex.Match(options, @"MusicDirectory, (.+)").Groups[1].Value.Trim();
                    DeviceName = Regex.Match(options, @"DeviceName, (.+)").Groups[1].Value.Trim();
                }
            }
        }
        #endregion
        #region UpdateTransferProgress
        /// <summary>
        /// Updates the progress of the transfer flowdocument
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateTransferProgress(string msg, bool addSeparator = false)
        {
            lock (TransferProgress)
            {
                if (addSeparator)
                {
                    TransferProgress += "-----------------------------------------------------";
                }
                TransferProgress += "\n";
                TransferProgress += $"{transferProgressLineCount} - {msg}";
            }
            Interlocked.Increment(ref transferProgressLineCount);
        }
        #endregion
        #region GetDeviceConnected
        /// <summary>
        /// Checks for connected device via MTP MediaDevice.dll
        /// </summary>
        internal void GetDeviceConnected()
        {
            DeviceName += " ";
            DeviceName = DeviceName.Trim();
            var devices = MediaDevice.GetDevices();
            try
            {
                var device = devices.First(d => d.FriendlyName == DeviceName);
                DeviceConnected = true;
                
            }
            catch { DeviceConnected = false; }
            finally 
            {
                //todo: need to figure out how to invoke update to Start Transfer button check Can_Execute (can delete the below)
                MusicDirectory += " ";
                MusicDirectory = MusicDirectory.Trim();
            }
        }
        #endregion
        #region GetIdv3Info(string path)
        /// <summary>
        /// Takes in a song object and adds idv3 tag info to
        /// </summary>
        /// <param name="song">the Song object to </param>
        public void GetIdv3Info(ref Song song)
        {
            //todo: maybe just add these to song when creating?
            TagLib.File songFile = TagLib.File.Create(Regex.Replace(song.Path, @"^\s*\.\.",MusicDirectory));
            song.Title = songFile.Tag.Title;
            song.Album = songFile.Tag.Album;
            song.Artists = songFile.Tag.Performers;
        }
        #endregion
        #region Changing ListView Stuff

        #region Loading Playlists in ListView
        internal void LoadPlaylists(List<string> playlistsFound)
        {
            foreach (string playlist in playlistsFound)
            {
                //MessageBox.Show(string.Format("Getting playlist object for path: {0}", playlist));
                ObservableCollection<Playlist> toAdd = GetPlaylists(playlist);
                foreach (Playlist playlist1 in toAdd)
                {
                    bool add = true;
                    foreach (Playlist playlist2 in lvPlaylistsList)
                    {
                        if (playlist2.Path == playlist1.Path)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                    {
                        lvPlaylistsList.Add(playlist1);
                    }
                }
            }
        }
        #endregion

        #region Remove Playlist in ListView
        internal void RemovePlayList(Playlist playlist)
        {
            lvPlaylistsList.Remove(playlist);
        }
        #endregion

        #region Remove All Playlist in ListView
        internal void RemovePlayListAll()
        {
            lvPlaylistsList.Clear();
        }
        #endregion

        #region Remove Song in ListView
        internal void RemoveSong(Song song)
        {
            lvSongsList.Remove(song);
        }
        #endregion

        #region Remove All Songs in ListView
        internal void RemoveSongAll()
        {
            lvSongsList.Clear();
        }
        #endregion

        #region Populating Songs in ListView
        internal void PopulateSongs(List<Playlist> playlists)
        {
            foreach (Playlist playlist in playlists)
            {
                bool add = true;
                ObservableCollection<Playlist> toAdd = new ObservableCollection<Playlist>();
                //MessageBox.Show(string.Format("playlist.Name: {0}", playlist.Name));
                foreach (Song song1 in playlist)
                {
                    foreach (Song song2 in lvSongsList)
                    {
                        if (song2.Path == song1.Path)
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add)
                    {
                        Song songToAdd = song1;
                        string path = Regex.Replace(song1.Path, @"\.\.", MusicDirectory);
                        bool fileExists = File.Exists(path);
                        if (fileExists)
                        {
                            GetIdv3Info(ref songToAdd);
                        }
                        else
                        {
                            songToAdd.Artists = new string[] { "File non-existant" };
                            songToAdd.Title = "File non-existant";
                            songToAdd.Path = song1.Path;
                        }
                        lvSongsList.Add(songToAdd);
                    }
                }
                //MessageBox.Show(string.Format("Adding: {0}", lvPlaylists.Items.IndexOf(lvPlaylists.SelectedItems[i])));
            }
        }
        #endregion

        #region Clearing ListViews
        public enum ListViewNamesEnum
        {
            Playlists,
            Songs
        }
        internal void ClearListView(ListViewNamesEnum lvName)
        {
            if (lvName == ListViewNamesEnum.Playlists)
            {
                lvPlaylistsList.Clear();
            }
            if (lvName == ListViewNamesEnum.Songs)
            {
                lvSongsList.Clear();
            }
        }
        #endregion

        #endregion
        #region ComparePlaylists
        /// <summary>
        /// Finds the difference between two playlists and returns an Observable collection of that difference.
        /// </summary>      
        /// <returns>two observablecollections representing the playlists unique to playlistone and playlist two. As an observable collection of Tuples where the string is the item and the int is the playlist to which it is unique</returns>
        public void ComparePlaylists(string playlistOnePath, string playlistTwoPath)
        {
            #region Initialization
            PlaylistOneLinesUnique.Clear();
            PlaylistTwoLinesUnique.Clear();
            PlaylistOneName = Path.GetFileName(playlistOnePath);
            PlaylistTwoName = Path.GetFileName(playlistTwoPath);
            string[] playlistOneLines = File.ReadAllLines(playlistOnePath);
            string[] playlistTwoLines = File.ReadAllLines(playlistTwoPath);
            HashSet<string> playlistOneLinesSet = new HashSet<string>();
            HashSet<string> playlistTwoLinesSet = new HashSet<string>();
            HashSet<string> playlistOneLinesSetUnique = new HashSet<string>();
            HashSet<string> playlistTwoLinesSetUnique = new HashSet<string>();
           
            ObservableCollection<Tuple<string, string>> difference = new ObservableCollection<Tuple<string, string>>();
            #endregion

            //todo: need to add to this region if adding more source or target playlists
            #region Playlist One Population
            if (playlistOnePath.Contains(SourcePlaylistTypes[(int)SourcePlaylistTypesEnum.m3u]))
            {
                foreach (string line in playlistOneLines)
                {
                    if (!line.Trim().StartsWith("#"))
                    {
                        playlistOneLinesSet.Add(line.Trim());
                    }
                }
            }
            else if (playlistOnePath.Contains(SourcePlaylistTypes[(int)SourcePlaylistTypesEnum.wpl]))
            {
                foreach (string line in playlistOneLines)
                {
                    playlistOneLinesSet.Add(GetPathOfMedia(line, SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum.m3u));
                }
            }
            #endregion
            #region Playlist Two Population
            if (playlistTwoPath.Contains(SourcePlaylistTypes[(int)SourcePlaylistTypesEnum.m3u]))
            {
                foreach (string line in playlistTwoLines)
                {
                    if (!line.Trim().StartsWith("#"))
                    {
                        playlistTwoLinesSet.Add(line.Trim());
                    }
                }
            }
            else if (playlistTwoPath.Contains(SourcePlaylistTypes[(int)SourcePlaylistTypesEnum.wpl]))
            {
                foreach (string line in playlistTwoLines)
                {
                    string lineToAdd = GetPathOfMedia(line, SourcePlaylistTypesEnum.wpl, TargetPlaylistTypesEnum.m3u);
                    if (!string.IsNullOrEmpty(lineToAdd))
                    {
                        lineToAdd = lineToAdd.Replace("/", "\\");
                        playlistTwoLinesSet.Add(lineToAdd);
                    }
                }
            }
            #endregion
            #region Creating Duplicate Sets
            foreach (string line in playlistOneLinesSet)
            {
                playlistOneLinesSetUnique.Add(line);
            }
            foreach (string line in playlistTwoLinesSet)
            {
                playlistTwoLinesSetUnique.Add(line);
            }
            #endregion
            #region Getting Difference and Populating ObservableCollection
            playlistOneLinesSetUnique.ExceptWith(playlistTwoLinesSet);
            playlistTwoLinesSetUnique.ExceptWith(playlistOneLinesSet);

            foreach (string line in playlistOneLinesSetUnique)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    PlaylistOneLinesUnique.Add(line);
                }
            }
            foreach (string line2 in playlistTwoLinesSetUnique)
            {
                if (!string.IsNullOrEmpty(line2))
                {
                    PlaylistTwoLinesUnique.Add(line2);
                }
            }
            #endregion
            #region Testing 
            //foreach (var item in difference)
            //{
            //    MessageBox.Show(string.Format("'{0}' is unique to playlist: \n{1}", item.Item1, item.Item2));
            //}
            #endregion
        }
        #endregion
        #endregion
    }
    #endregion
}

