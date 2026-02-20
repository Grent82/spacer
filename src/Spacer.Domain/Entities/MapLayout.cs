namespace Spacer.Domain.Entities;

using System;
using System.Collections.Generic;
using Spacer.Domain.ValueObjects;

public sealed class MapLayout
{
    public MapLayout(int width, int height, int[] cells, IReadOnlyList<Position> defaultPositions)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }
        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }
        if (cells is null)
        {
            throw new ArgumentNullException(nameof(cells));
        }
        if (cells.Length != width * height)
        {
            throw new ArgumentException("Cell count does not match map dimensions.", nameof(cells));
        }

        Width = width;
        Height = height;
        Cells = cells;
        DefaultPositions = defaultPositions ?? Array.Empty<Position>();
    }

    public int Width { get; }
    public int Height { get; }
    public int[] Cells { get; }
    public IReadOnlyList<Position> DefaultPositions { get; }

    public int GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return 0;
        }

        return Cells[y * Width + x];
    }
}
