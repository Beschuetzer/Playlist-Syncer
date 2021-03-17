using System.Windows.Input;

namespace PlaylistsMadeEasy
{
    class CustomCommands
    {
        public static readonly RoutedCommand Exit = new RoutedUICommand
            (
                "Exit",
                "Exit",
                typeof(CustomCommands),
                new InputGestureCollection
                {
                    new KeyGesture(Key.F4, ModifierKeys.Alt)            //This is how you define keyboard shortcuts
                }
            );

        public static readonly RoutedCommand GetPlaylistPath = new RoutedUICommand
            (
                "Opens a fileDialogBox to get the path to the playlists.",
                "GetPlaylistPath",
                typeof(CustomCommands),
                new InputGestureCollection
                {
                    new KeyGesture(Key.P, ModifierKeys.Control)            //This is how you define keyboard shortcuts
                }
            );

        public static readonly RoutedCommand GetMusicPath = new RoutedUICommand
            (
                "Opens a fileDialogBox to get the path to the Music contained in the Playlists.",
                "GetMusicPath",
                typeof(CustomCommands),
                new InputGestureCollection
                {
                    new KeyGesture(Key.M, ModifierKeys.Control)            //This is how you define keyboard shortcuts
                }
            );

        public static readonly RoutedCommand SelectPaths = new RoutedUICommand
           (
               "Selects both Paths via OpenFolderDialog",
               "SelectPaths",
               typeof(CustomCommands),
               new InputGestureCollection
               {
                    new KeyGesture(Key.B, ModifierKeys.Control)            //This is how you define keyboard shortcuts
               }
           );

        public static readonly RoutedCommand StartTransfer = new RoutedUICommand
          (
              "Starts the whole process",
              "StartTransfer",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.S, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
          public static readonly RoutedCommand LoadPlaylists = new RoutedUICommand
          (
              "Loads playlists into list view",
              "LoadPlaylists",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.L, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
        public static readonly RoutedCommand RemoveSelected = new RoutedUICommand
          (
              "Removes selected playlists from lvPlaylists",
              "RemoveSelected",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.R, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
        public static readonly RoutedCommand RemoveAll = new RoutedUICommand
          (
              "Removes all playlists from lvPlaylists",
              "RemoveAll",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.A, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
        public static readonly RoutedCommand ViewSongs = new RoutedUICommand
          (
              "Views the song names in the selected playlist",
              "ViewSongs",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.V, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
        public static readonly RoutedCommand EditSongs = new RoutedUICommand
          (
              "Opens a prompt to edit the selected songs",
              "EditSongs",
              typeof(CustomCommands),
              new InputGestureCollection
              {
                    new KeyGesture(Key.E, ModifierKeys.Control),            //This is how you define keyboard shortcuts
              }
          );
        public static readonly RoutedCommand GetDifferenceBetweenPlaylists = new RoutedUICommand
         (
             "Finds the differences between playlists",
             "GetDifferenceBetweenPlaylists",
             typeof(CustomCommands),
             new InputGestureCollection
             {
                    new KeyGesture(Key.D, ModifierKeys.Control),            //This is how you define keyboard shortcuts
             }
         );
        public static readonly RoutedCommand WritePreferences = new RoutedUICommand
         (
             "Write Preferences to file",
             "WritePreferences",
             typeof(CustomCommands),
             new InputGestureCollection
             {
                    new KeyGesture(Key.W, ModifierKeys.Control),            //This is how you define keyboard shortcuts
             }
         );
    }
}
