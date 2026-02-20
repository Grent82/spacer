namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class CsvMapLayoutRepository : IMapLayoutRepository
{
    private const int GridSize = 21;
    private const int DefaultEntryLimit = 100;

    private readonly string _mainMapPath;
    private readonly string _defMapPath;
    private MapLayout? _layout;

    public CsvMapLayoutRepository(string mainMapPath, string defMapPath)
    {
        _mainMapPath = mainMapPath ?? string.Empty;
        _defMapPath = defMapPath ?? string.Empty;
    }

    public MapLayout GetLayout()
    {
        if (_layout is not null)
        {
            return _layout;
        }

        var cells = LoadMainMap(_mainMapPath);
        var defaults = LoadDefaultPositions(_defMapPath);
        _layout = new MapLayout(GridSize, GridSize, cells, defaults);
        return _layout;
    }

    private static int[] LoadMainMap(string path)
    {
        var cells = new int[GridSize * GridSize];
        if (!File.Exists(path))
        {
            return cells;
        }

        var values = new List<int>(cells.Length);
        foreach (var row in CsvRowReader.ReadRows(path))
        {
            if (row.Length == 0)
            {
                continue;
            }

            foreach (var cell in row)
            {
                if (int.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    values.Add(value);
                }
            }
        }

        var limit = Math.Min(cells.Length, values.Count);
        for (var i = 0; i < limit; i++)
        {
            cells[i] = values[i];
        }

        return cells;
    }

    private static IReadOnlyList<Position> LoadDefaultPositions(string path)
    {
        var positions = new List<Position>();
        if (!File.Exists(path))
        {
            return positions;
        }

        foreach (var row in CsvRowReader.ReadRows(path))
        {
            if (row.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(row[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x))
            {
                continue;
            }

            if (!int.TryParse(row[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
            {
                continue;
            }

            positions.Add(new Position(x, y));
            if (positions.Count >= DefaultEntryLimit)
            {
                break;
            }
        }

        return positions;
    }
}
