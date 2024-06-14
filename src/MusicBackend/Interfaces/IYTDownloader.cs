using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Model;

namespace MusicBackend.Interfaces;

internal interface IYTDownloader
{
    Task<Song> DownloadVideoAsync(string url, Action<double> progressFunc);
}
