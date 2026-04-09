using System.Text.Json.Serialization;

namespace Masiro.Models.Novel;

public class TagInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
