using System.Text.Json;

namespace Base.Helpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions CamelCase = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}