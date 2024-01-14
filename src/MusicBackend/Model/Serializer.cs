using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicBackend.Model;
public class Serializer
{
    [JsonInclude]
    internal QueueModelState? QueueState {  get; set; }
    [JsonInclude]
    internal PlayerModelState? PlayerState { get; set; }
    [JsonInclude]
    internal PlaylistManagerState? PlaylistState {  get; set; }
    [JsonInclude]
    internal YTDownloaderCacheState? YTDownloaderCacheState { get; set; }
    public static void Deserialize()
    {
        Serializer appState;
        var (statepath, jsonOptions) = CreateConfig();
        try
        {
            var data = File.ReadAllText(statepath);
            appState = JsonSerializer.Deserialize<Serializer>(data,jsonOptions);
            if (appState is null)
            {
                appState = new();
            }
        } catch
        {
            appState = new();
        }

        if (appState.QueueState is not null)
        {
			QueueModel.InitWithState(appState.QueueState);
		}
        if (appState.PlayerState is not null)
        {
            PlayerModel.InitWithState(appState.PlayerState);
        }
        if (appState.PlaylistState is not null)
        {
			PlaylistManager.InitWithState(appState.PlaylistState);
		}
        if (appState.YTDownloaderCacheState is not null)
        {
            SongManager.Instance.InitYtCache(appState.YTDownloaderCacheState);
        }
    }

    private static (string,JsonSerializerOptions) CreateConfig()
    {
        var exedir = AppDomain.CurrentDomain.BaseDirectory;
        var statepath = Path.Combine(exedir, "state.json");
        var jsonOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        return (statepath,jsonOptions);
    }

    public static void Serialize()
    {
        var (statepath, jsonOptions) = CreateConfig();

        var appState = new Serializer()
        {
			QueueState = QueueModel.Instance.DumpState(),
			PlayerState = PlayerModel.Instance.DumpState(),
			PlaylistState = PlaylistManager.Instance.DumpState(),
            YTDownloaderCacheState = SongManager.Instance.DumpYtCache()
		};

		var str = JsonSerializer.Serialize(appState,jsonOptions);
		File.WriteAllText(statepath,str);
    }
}
