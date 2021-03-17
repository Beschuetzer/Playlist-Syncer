using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistsMadeEasy
{
    #region Song Class
    /// <summary>
    /// Represents a Song
    /// </summary>
    public class Song
    {
        public string Name { get; set; }
        public string PlaylistName { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public string Album { get; set; }
    }
    #endregion
}
