using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Interfaces;

internal interface IYTDownloader
{
	Task<Song> DownloadVideoAsync(string url, Action<double> progressFunc);
}
