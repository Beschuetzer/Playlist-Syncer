using System.Collections.ObjectModel;
using System.Windows;

namespace PlaylistsMadeEasy
{
    /// <summary>
    /// Interaction logic for ComparePlaylistsWindow.xaml
    /// </summary>
    public partial class ComparePlaylistsWindow : Window
    {
        public string PlaylistOneName;
        public string PlaylistTwoName;
        public ComparePlaylistsWindow(ObservableCollection<string> plOne, ObservableCollection<string> plTwo, string plOneName, string plTwoName)
        {
            InitializeComponent();
            lvPlaylistOne.ItemsSource = plOne;
            lvPlaylistTwo.ItemsSource = plTwo;
            lbl_PlaylistOneName.Content = plOneName;
            lbl_PlaylistTwoName.Content = plTwoName;
            this.Title = string.Format("Comparing Playlist '{0}' to '{1}'", plOneName, plTwoName);
            if (plOne.Count > 16 || plTwo.Count > 16)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
