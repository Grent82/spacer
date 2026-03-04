namespace Spacer.Infrastructure.Config;

using System.IO;
using System.Text.Json;

public sealed record ScenarioTimeConfig(int StartYear, int StartMonth);

public static class ScenarioTimeConfigLoader
{
    private sealed record ScenarioTimeDto(int StartYear, int StartMonth);

    public static ScenarioTimeConfig Load(string path, int fallbackYear, int fallbackMonth)
    {
        if (!File.Exists(path))
        {
            return new ScenarioTimeConfig(fallbackYear, fallbackMonth);
        }

        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<ScenarioTimeDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (dto is null)
        {
            return new ScenarioTimeConfig(fallbackYear, fallbackMonth);
        }

        var year = dto.StartYear <= 0 ? fallbackYear : dto.StartYear;
        var month = dto.StartMonth <= 0 ? fallbackMonth : dto.StartMonth;
        return new ScenarioTimeConfig(year, month);
    }
}
