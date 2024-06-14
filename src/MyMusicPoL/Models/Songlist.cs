using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Model;

namespace mymusicpol.Models
{
    // for now, this is just a placeholder class
    internal class Songlist
    {
        public string name { get; set; }
        public List<Song> songs { get; set; }

        public Songlist(string name, List<Song> songs)
        {
            this.name = name;
            this.songs = songs;
        }

        public void AddSong(Song song)
        {
            songs.Add(song);
        }

        public void RemoveSong(Song song)
        {
            songs.Remove(song);
        }
    }
}
