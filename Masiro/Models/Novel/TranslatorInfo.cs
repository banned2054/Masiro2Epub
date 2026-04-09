using System.Text.Json.Serialization;

namespace Masiro.Models.Novel;

public class TranslatorInfo
{
    [JsonPropertyName("translator")]
    public int TranslatorId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
