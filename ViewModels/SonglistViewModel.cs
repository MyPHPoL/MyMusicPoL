﻿using mymusicpol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymusicpol.ViewModels
{
	internal class SonglistViewModel : ViewModelBase
	{
		public readonly Songlist songlist;
		public string name => songlist.name;
		public List<Song> songs => songlist.songs;

		public SonglistViewModel(Songlist songlist)
		{
			this.songlist = songlist;
		}
	}
}