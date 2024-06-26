﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Model;
using mymusicpol.Models;

namespace mymusicpol.ViewModels
{
    internal class SonglistViewModel : ViewModelBase
    {
        private string name;
        private ObservableCollection<Song> songs;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public SonglistViewModel(
            string name,
            List<MusicBackend.Model.Song> songs
        )
        {
            this.name = name;
            this.songs = new();
            SetSongs(songs);
        }

        public void SetSongs(List<MusicBackend.Model.Song> songs)
        {
            foreach (var song in songs)
            {
                this.songs.Add(song);
            }
        }

        public ObservableCollection<Song> Songs
        {
            get => songs;
            set
            {
                songs = value;
                OnPropertyChanged(nameof(Songs));
            }
        }
    }
}
