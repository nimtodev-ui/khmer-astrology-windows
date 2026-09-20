using System.Text.Json;

namespace KhmerAstrology.Infrastructure.ReferenceData;

internal static class JsonFile
{
    public static T Read<T>(string path, string description)
        where T : class
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{description} reference data was not deployed.", path);
        }

        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });
        return value ?? throw new InvalidDataException($"{description} reference data is invalid.");
    }

    public static IReadOnlyList<T> ReadArray<T>(
        string path,
        string description,
        Func<JsonElement, T> factory)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{description} reference data was not deployed.", path);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"{description} reference data must be a JSON array.");
        }

        return document.RootElement.EnumerateArray().Select(factory).ToArray();
    }
}
