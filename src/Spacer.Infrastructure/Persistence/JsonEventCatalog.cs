namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Spacer.Application.Events;
using Spacer.Application.Ports;

public sealed class JsonEventCatalog : IEventCatalog
{
    private readonly string _directoryPath;
    private readonly JsonSerializerOptions _options;
    private readonly Dictionary<string, EventDefinition> _cache;
    private readonly Dictionary<string, string> _index;
    private bool _indexBuilt;

    public JsonEventCatalog(string directoryPath)
    {
        _directoryPath = directoryPath;
        _cache = new Dictionary<string, EventDefinition>(StringComparer.OrdinalIgnoreCase);
        _index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    public IReadOnlyList<EventDefinition> GetAll()
    {
        return LoadAll();
    }

    public IReadOnlyList<EventDefinition> GetByTrigger(string trigger)
    {
        return Array.Empty<EventDefinition>();
    }

    public EventDefinition? FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (_cache.TryGetValue(id, out var cached))
        {
            return cached;
        }

        EnsureIndex();
        if (!_index.TryGetValue(id, out var path))
        {
            return null;
        }

        var loaded = LoadEvent(path);
        if (loaded is null)
        {
            return null;
        }

        _cache[id] = loaded;
        return loaded;
    }

    private IReadOnlyList<EventDefinition> LoadAll()
    {
        var events = new List<EventDefinition>();
        EnsureIndex();
        foreach (var path in _index.Values)
        {
            var definition = LoadEvent(path);
            if (definition is null)
            {
                continue;
            }
            events.Add(definition);
            _cache[definition.Id] = definition;
        }

        return events;
    }

    private void EnsureIndex()
    {
        if (_indexBuilt)
        {
            return;
        }

        _indexBuilt = true;
        if (!Directory.Exists(_directoryPath))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(_directoryPath, "*.json", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(path);
            if (string.Equals(fileName, "event.schema.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var id = Path.GetFileNameWithoutExtension(path);
            if (_index.ContainsKey(id))
            {
                throw new InvalidOperationException($"Duplicate event id '{id}' from '{path}'.");
            }

            _index[id] = path;
        }
    }

    private EventDefinition? LoadEvent(string path)
    {
        var id = Path.GetFileNameWithoutExtension(path);
        var json = File.ReadAllText(path);
        var definition = JsonSerializer.Deserialize<EventDefinition>(json, _options);
        if (definition is null)
        {
            return null;
        }

        if (definition.Steps.Count == 0)
        {
            throw new InvalidOperationException($"Event '{path}' has no steps.");
        }

        return definition with { Id = id, Source = path };
    }
}
