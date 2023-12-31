using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace mymusicpol.Models
{
	// for now, this is just a placeholder class
	internal class Song
	{
		public string title { get; set; }
		public string artist { get; set; }
		public SongAlbum album { get; set; }



		public Song(MusicBackend.Model.Song song)
		{
			this.title = song.name;
			this.artist = song.artist;
			this.album = song.Album;
		}

	}
}
