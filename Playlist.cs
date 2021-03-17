using System.Collections;
using System.Collections.Generic;
using static PlaylistsMadeEasy.PlaylistManager;

namespace PlaylistsMadeEasy
{
    #region Playlist Class
    /// <summary>
    /// Represents a Playlist
    /// </summary>
    public class Playlist
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public HashSet<Song> Songs = new HashSet<Song>();
        public HashSet<string> SongNames = new HashSet<string>();
        public HashSet<string> SongPathesToCopy = new HashSet<string>();
        public SourcePlaylistTypesEnum Type;
        public void AddSongName(string songName)
        {
            SongNames.Add(songName);
        }
        public IEnumerator GetEnumerator()
        {
            foreach (Song song in Songs)
            {
                // Return one month per iteration. 
                yield return song;
            }
        }
    }
    #endregion
}
