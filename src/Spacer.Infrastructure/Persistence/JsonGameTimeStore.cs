namespace Spacer.Infrastructure.Persistence;

using System.IO;
using System.Text.Json;
using Spacer.Application.GameState;
using Spacer.Application.Ports;

public sealed class JsonGameTimeStore : IGameTimeStore
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options;

    public JsonGameTimeStore(string path)
    {
        _path = path;
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public GameTimeSnapshot? Load()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<GameTimeSnapshot>(json, _options);
    }

    public void Save(GameTimeSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(snapshot, _options);
        File.WriteAllText(_path, json);
    }
}
