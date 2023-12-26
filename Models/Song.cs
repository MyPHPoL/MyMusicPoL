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
		private string title { get; set; } = "Unknown Title";
		private string artist { get; set; } = "Unknown Artist";
		private string album { get; set; } = "Unknown Album";
		private string path { get; set; }
		private string? cover { get; set; }

		public Song(string title, string artist, string album, string path, string cover)
		{
			this.title = title;
			this.artist = artist;
			this.album = album;
			this.path = path;
			this.cover = cover;
		}

	}
}
