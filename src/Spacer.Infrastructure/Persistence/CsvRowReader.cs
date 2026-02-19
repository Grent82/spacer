namespace Spacer.Infrastructure.Persistence;

using System.Collections.Generic;
using System.IO;
using System.Text;

public static class CsvRowReader
{
    public static IEnumerable<string[]> ReadRows(string path, Encoding? encoding = null)
    {
        if (!File.Exists(path))
        {
            yield break;
        }

        if (encoding is not null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        using var reader = encoding is null
            ? new StreamReader(path)
            : new StreamReader(path, encoding, detectEncodingFromByteOrderMarks: true);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("#", StringComparison.Ordinal)
                || trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            yield return ParseLine(line);
        }
    }

    public static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
