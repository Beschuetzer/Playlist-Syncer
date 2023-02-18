using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using TagLib.Ape;
using static PlaylistsMadeEasy.PlaylistManager;
using Application = System.Windows.Application;
using File = System.IO.File;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PlaylistsMadeEasy
{
    public partial class MainWindow : Window
    {
        #region Members
        private bool MainWindowInstantiated = false;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        
        #endregion
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            InstantiateObject();
        }
        #endregion
        #region InstantiateObject
        private void InstantiateObject()
        {
            #region Initialization Stuff
            PlaylistManager plm = new PlaylistManager()
            {
                ConfirmDelete = false,
                DeleteBeforeWriting = true,
            };
            lvSongs.ItemsSource = plm.lvSongsList;
            lvPlaylists.ItemsSource = plm.lvPlaylistsList;

            #region Setting ComboBox sources and Initial Selections
            ComboBox_SourcePlaylistType.ItemsSource = PlaylistManager.SourcePlaylistTypes;
            ComboBox_TargetPlaylistType.ItemsSource = PlaylistManager.TargetPlaylistTypes;
            ComboBox_SourcePlaylistType.SelectedIndex = 0;
            ComboBox_TargetPlaylistType.SelectedIndex = 0;
            #endregion

            #region Instantiating PlaylistManager, Loading Previous Pathes, and Setting it to this.DataContext
            Textbox_DeviceName.Text = "";
            
            MainWindowInstantiated = true;
            //Loading last pathes
            plm.GetPreferencesFromFile();
            this.DataContext = plm;
            #endregion
            #endregion
        }
        #endregion
        #region CommandBinding_Executed
        /// <summary>
        /// What to do when UIElement is executable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)      //These two methods are used by RoutedCommand and RoutedUICommand.  Put in the main Window class
        {
            String Name = ((RoutedCommand)e.Command).Name;

            if (Name == "Exit")
            {
                Application.Current.Shutdown();
            }
            else if (Name == "GetPlaylistPath")
            {
                GetPlaylistPath();
            }
            else if (Name == "GetMusicPath")
            {
                GetMusicPath();
            }
            else if (Name == "SelectPaths")
            {
                GetPlaylistPath();
                GetMusicPath();
            }
            else if (Name == "StartTransfer")
            {
                #region Setting Up Stuff
                #region Creating new PlaylistManager from datacontext
                PlaylistManager plm = this.DataContext as PlaylistManager;
                #endregion
                #region Setting the targetType from ComboBox
                TargetPlaylistTypesEnum targetType;
                switch (ComboBox_TargetPlaylistType.SelectedIndex)
                {
                    case 0:
                        targetType = TargetPlaylistTypesEnum.m3u;
                        break;
                    default:
                        targetType = TargetPlaylistTypesEnum.m3u;
                        break;
                }
                plm.TargetPlaylistType = targetType;
                #endregion
                #region Setting DestinationOnPhone
                if (RadioButton_Phone.IsChecked == true)
                {
                    plm.DestinationOnPhone = DestinationOnPhoneLocationsEnum.Phone;
                }
                else
                {
                    plm.DestinationOnPhone = DestinationOnPhoneLocationsEnum.Card;
                }
                #endregion
                #region Writing Preferences to File
                plm.WritePreferencesToFiles(Textbox_PlaylistPath.Text, Textbox_MusicPath.Text, Textbox_DeviceName.Text);
                #endregion
                #endregion
                #region Getting the Paths of Playlists to Transfer
                List<string> playlistsToTransfer = new List<string>();
                foreach (Playlist item in lvPlaylists.SelectedItems)
                {
                    playlistsToTransfer.Add(item.Path);
                }
                #endregion
                #region Transferring
                if (playlistsToTransfer.Count > 0)
                {
                    plm.StartTransfer(playlistsToTransfer);
                    lvPlaylists.UnselectAll();
                }
                else
                {
                    MessageBox.Show(string.Format("Please Select at Least One Playlist."), string.Format("No Playlists Selected"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                #endregion
            }
            else if (Name == "LoadPlaylists")
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                GetSourcePlaylistType();
                #region Checking to see if any playlists of the source type exist and then loading them
                List<string> playlistsFound = Directory.GetFiles(Textbox_PlaylistPath.Text, "*" + SourcePlaylistTypes[(int)plm.SourcePlaylistType], SearchOption.AllDirectories).ToList();

                #region If Playlists Found
                if (playlistsFound.Count > 0)
                {
                    #region  Get the list of Playlists to load
                    //todo: idea - have one LV showing playlists and song count, then when you click on it, it shows all the songs in the playlist in another LV?
                    plm.LoadPlaylists(playlistsFound);

                    #region Testing playlistsToLoad
                    //foreach (var playlist in lvPlaylistsList)
                    //{
                    //    MessageBox.Show(string.Format("playlist.name: {0}\n Song.COunt: {1}", playlist.Name, playlist.Songs.Count));
                    //}
                    #endregion                   
                    #endregion
                }
                #endregion

                #region If No playlists Found
                else
                {
                    MessageBox.Show(string.Format($"No playlists of type .{plm.SourcePlaylistType} exist at '{plm.DirectoryToPlaylists}'.  Please select a different directory and/or a different playlist source type."), string.Format("No Playlists Found"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                #endregion

                #endregion
            }
            else if (Name == "RemoveSelected")
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                if (e.OriginalSource.ToString().Contains(".Playlist"))
                {
                    #region Getting the Playlist objects to Remove (Can't enumerate on a changed set [i.e.  lvPlaylists.SelectedItems])
                    List<Playlist> toRemove = new List<Playlist>();
                    foreach (Playlist playlist in lvPlaylists.SelectedItems)
                    {
                        toRemove.Add(playlist);
                        //MessageBox.Show(string.Format("Adding: {0}", lvPlaylists.Items.IndexOf(lvPlaylists.SelectedItems[i])));
                    }
                    #endregion

                    #region Sending Playlists to PLM for Removal
                    foreach (Playlist playlist1 in toRemove)
                    {
                        plm.RemovePlayList(playlist1);
                    }
                    #endregion
                }
                else if (e.OriginalSource.ToString().Contains(".Song"))
                {
                    #region Getting the Playlist objects to Remove (Can't enumerate on a changed set [i.e.  lvPlaylists.SelectedItems])
                    List<Song> toRemove = new List<Song>();
                    foreach (Song song in lvSongs.SelectedItems)
                    {
                        toRemove.Add(song);
                        //MessageBox.Show(string.Format("Adding: {0}", lvPlaylists.Items.IndexOf(lvPlaylists.SelectedItems[i])));
                    }
                    #endregion

                    #region Sending Playlists to PLM for Removal
                    foreach (Song song1 in toRemove)
                    {
                        plm.RemoveSong(song1);
                    }
                    #endregion
                }
            }
            else if (Name == "RemoveAll")
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                if (e.OriginalSource.ToString().Contains(".Playlist"))
                {
                    plm.RemovePlayListAll();
                }
                else if (e.OriginalSource.ToString().Contains(".Song"))
                {
                    plm.RemoveSongAll();
                }
            }
            else if (Name == "ViewSongs")
            {
                //todo: mainly use idv3 tag as header info (Artist, title, album, path)
                PlaylistManager plm = this.DataContext as PlaylistManager;
                List<Playlist> playlistsSelected = new List<Playlist>();
                foreach (Playlist playlist in lvPlaylists.SelectedItems)
                {
                    playlistsSelected.Add(playlist);
                }
                plm.PopulateSongs(playlistsSelected);
            }
            else if (Name == "EditSongs")
            {
                //Get the songs from lvPlaylists selecteditems playlist objects
                List<Playlist> toAdd = new List<Playlist>();
                foreach (Playlist playlist in lvPlaylists.SelectedItems)
                {
                    toAdd.Add(playlist);
                    //MessageBox.Show(string.Format("Adding: {0}", lvPlaylists.Items.IndexOf(lvPlaylists.SelectedItems[i])));
                }
                //Change the lvSongsList collection
                if (lvSongs.SelectedItems.Count > 0)
                {

                }
            }
            else if (Name == "GetDifferenceBetweenPlaylists")
            {
                int i = 0;
                string playlistOnePath = null;
                string playlistTwoPath = null;
                PlaylistManager plm = this.DataContext as PlaylistManager;
                foreach (Playlist playlist in lvPlaylists.SelectedItems)
                {
                    if (i == 0)
                    {
                        playlistOnePath = playlist.Path;
                    }
                    else
                    {
                        playlistTwoPath = playlist.Path;
                    }
                    i++;
                    //MessageBox.Show(string.Format("playlist: {0}", playlist.Path));
                }
                plm.ComparePlaylists(playlistOnePath, playlistTwoPath);
                ComparePlaylistsWindow comparePlaylistsWindow = new ComparePlaylistsWindow(plm.PlaylistOneLinesUnique, plm.PlaylistTwoLinesUnique, plm.PlaylistOneName, plm.PlaylistTwoName);
                comparePlaylistsWindow.ShowDialog();
            }
            else if (Name == "WritePreferences")
            {
                #region Changing CheckBoxes
                if (CheckBox_SavePlaylistLocation.IsChecked == true)
                {
                    CheckBox_SavePlaylistLocation.IsChecked = false;
                }
                else
                {
                    CheckBox_SavePlaylistLocation.IsChecked = true;
                }
                if (CheckBox_SaveMusicLocation.IsChecked == true)
                {
                    CheckBox_SaveMusicLocation.IsChecked = false;
                }
                else
                {
                    CheckBox_SaveMusicLocation.IsChecked = true;
                }
                #endregion
                PlaylistManager plm = new PlaylistManager();
                plm.MenuItem_RememberPathes_Flag = MenuItem_RememberPathes.IsChecked;
                plm.WritePreferencesToFiles(Textbox_PlaylistPath.Text, Textbox_MusicPath.Text, Textbox_DeviceName.Text);
            }
        }
        #endregion
        #region CommandBinding_CanExecute
        /// <summary>
        /// Needed for Determining whether UIElement is enabled and canExecute        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            string Name = ((RoutedCommand)e.Command).Name;
            if (Name == "GetPlaylistPath")
            {
                e.CanExecute = true;
            }
            else if (Name == "StartTransfer")
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                if (Directory.Exists(Textbox_PlaylistPath.Text) && Directory.Exists(Textbox_MusicPath.Text) && !string.IsNullOrEmpty(Textbox_DeviceName.Text) && lvPlaylists.SelectedItems.Count > 0 && plm.Transfer_Started_Flag == false && plm.DeviceConnected)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "LoadPlaylists")
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                if (ComboBox_SourcePlaylistType.SelectedItem != null && Directory.Exists(Textbox_PlaylistPath.Text) && plm.DirectoryToPlaylistsHasFiles == true)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "RemoveAll")
            {
                if (lvPlaylists.Items.Count > 0)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "RemoveSelected")
            {
                if (lvPlaylists.SelectedItems.Count > 0)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "ViewSongs")
            {
                if (lvPlaylists.SelectedItems.Count > 0)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "EditSongs")
            {
                if (lvSongs.SelectedItems.Count > 0)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else if (Name == "GetDifferenceBetweenPlaylists")
            {
                if (lvPlaylists.SelectedItems.Count == 2)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }               
            else
            {
                e.CanExecute = true;
            }
        }
        #endregion
        #region Other Methods
        #region GetFolderviaDialogBox
        private string GetFolderviaDialogBox(string folderName)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = true;
                folderBrowserDialog.Description = $"Select the {folderName} Folder";
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }
                return null;
            }
        }
        #endregion
        #region ColumnHeader Event for Adorner Class
        private void lvPlaylistsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            bool IslvPlaylists = e.Source.ToString().Contains("Path") || e.Source.ToString().Contains("Playlist");
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);

                if (IslvPlaylists)
                {
                    lvPlaylists.Items.SortDescriptions.Clear();
                }
                else
                {
                    lvSongs.Items.SortDescriptions.Clear();
                }
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            if (IslvPlaylists)
            {
                lvPlaylists.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
            else
            {
                lvSongs.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }
        #endregion
        #region GetSourcePlaylistType
        /// <summary>
        /// Sets the SourcePlaylist type for the datacontext object.  Needed as can't figure out binding to a combobox selection
        /// </summary>
        public void GetSourcePlaylistType()
        {
            #region Getting sourceType from ComboBox
            if (MainWindowInstantiated)
            {
                PlaylistManager plm = this.DataContext as PlaylistManager;
                SourcePlaylistTypesEnum sourceType;
                //todo: need to add more source types, add options here (they match up with sourcePlaylistTypes)
                switch (ComboBox_SourcePlaylistType.SelectedIndex)
                {
                    case 0:
                        sourceType = SourcePlaylistTypesEnum.wpl;
                        break;
                    case 1:
                        sourceType = SourcePlaylistTypesEnum.m3u;
                        break;
                    default:
                        sourceType = SourcePlaylistTypesEnum.wpl;
                        break;
                }
                #endregion
                plm.SourcePlaylistType = sourceType;
            }
        }
        #endregion
        #region Event Handlers
        #region textbox_GotFocus
        /// <summary>
        /// Clears textbox when it gets focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = ((System.Windows.Controls.TextBox)sender);
            if (textBox.Text == _PLAYLIST_TEXTBOX_STARTINGTEXT || textBox.Text == _MUSIC_TEXTBOX_STARTINGTEXT || textBox.Text == _ERROR_PLAYLIST_PATH || textBox.Text == _ERROR_MUSIC_PATH || textBox.Text == _DEVICENAME_TEXTBOX_STARTINGTEXT)
            {
                textBox.Text = "";
            }
        }
        #endregion
        private void GetPlaylistPath()
        {
            string path = GetFolderviaDialogBox("Playlist");
            Textbox_PlaylistPath.Text = path != null ? path : _ERROR_PLAYLIST_PATH;
        }
        private void GetMusicPath()
        {
            string path = GetFolderviaDialogBox("Music");
            Textbox_MusicPath.Text = path != null ? path : _ERROR_MUSIC_PATH;
        }

        /// <summary>
        /// Delegate Necessary to invoke GetSourcePlaylistType()
        /// </summary>
        private void ComboBox_SourcePlaylistType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSourcePlaylistType();
            Textbox_PlaylistPath.Text += " ";
            Textbox_PlaylistPath.Text = Textbox_PlaylistPath.Text.Trim();
        }

        /// <summary>
        ///Needed to use the mouse scroll wheel for the Scrollviewers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvPlaylists_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta /1.3));
            e.Handled = true;
        }

        /// <summary>
        /// Loads song for selected playlist on dbl click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvPlaylists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //todo: finish dbl click loads selected songs from playlists
            //todo: add playlist column to song viewer?
            PlaylistManager plm = this.DataContext as PlaylistManager;
            System.Windows.Controls.ListView lv = sender as System.Windows.Controls.ListView;
            Playlist playlist = lv.SelectedItem as Playlist;
            List<Playlist> toShow = new List<Playlist>();
            toShow.Add(playlist);
            plm.PopulateSongs(toShow);
        }
    }
    #endregion
        #endregion
        #region SortAdorner Class
        public class SortAdorner : Adorner
        {
            private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir) : base(element)
            {
                this.Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                TranslateTransform transform = new TranslateTransform
                    (
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2
                    );
                drawingContext.PushTransform(transform);

                Geometry geometry = ascGeometry;
                if (this.Direction == ListSortDirection.Descending)
                    geometry = descGeometry;
                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        //todo: fix transfer progress formatting?
    }
     #endregion
}
