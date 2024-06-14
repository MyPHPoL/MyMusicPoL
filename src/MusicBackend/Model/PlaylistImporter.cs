using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace MusicBackend.Model;

interface IPlaylistImportExport
{
    public string Name { get; set; }
    public string[] Songs { get; set; }
    bool Import(string path);
    void Export(string path);
}

// must be public for xml serializer to work
public class PlaylistXML : IPlaylistImportExport
{
    public string Name { get; set; }
    public string[] Songs { get; set; }

    public void Export(string path)
    {
        var serializer = new XmlSerializer(typeof(PlaylistXML));
        var writer = new StreamWriter(path);
        serializer.Serialize(writer, this);
    }

    public bool Import(string path)
    {
        var serializer = new XmlSerializer(typeof(PlaylistXML));
        var reader = new StreamReader(path);
        var playlist = serializer.Deserialize(reader) as PlaylistXML;
        if (playlist is null)
        {
            return false;
        }
        Name = playlist.Name;
        Songs = playlist.Songs;
        return true;
    }
}

internal class PlaylistJSON : IPlaylistImportExport
{
    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public string[] Songs { get; set; }
    private JsonSerializerOptions jsonOptions =
        new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

    public void Export(string path)
    {
        var data = JsonSerializer.Serialize(this, jsonOptions);
        File.WriteAllText(path, data);
    }

    public bool Import(string path)
    {
        try
        {
            var data = File.ReadAllText(path);
            var playlist = JsonSerializer.Deserialize<PlaylistJSON>(
                data,
                jsonOptions
            );
            if (playlist is null)
            {
                return false;
            }
            Name = playlist.Name;
            Songs = playlist.Songs;
        }
        catch
        {
            return false;
        }

        return true;
    }
}

internal class PlaylistImporter
{
    IPlaylistImportExport strategy;
    string path;

    public PlaylistImporter(string path)
    {
        this.path = path;
        if (path.EndsWith(".xml"))
        {
            strategy = new PlaylistXML();
        }
        else if (path.EndsWith(".json"))
        {
            strategy = new PlaylistJSON();
        }
        else
        {
            throw new Exception("Unknown file type");
        }
    }

    public Playlist? Import()
    {
        var res = strategy.Import(path);
        if (res is false)
        {
            return null;
        }

        var playlist = new Playlist(strategy.Name);
        foreach (var songPath in strategy.Songs)
        {
            var song = Song.fromPath(
                Path.Combine(LibraryManager.MusicPath, songPath)
            );
            if (song is not null)
            {
                playlist.Songs.Add(song);
            }
        }
        return playlist;
    }

    public void Export(Playlist playlist)
    {
        strategy.Name = playlist.Name;
        strategy.Songs = playlist
            .Songs.Select(x =>
                x.path.Replace(LibraryManager.MusicPath + '\\', "")
            )
            .ToArray();
        strategy.Export(path);
    }
}
