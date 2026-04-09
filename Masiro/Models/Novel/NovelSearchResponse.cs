using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Masiro.Models.Novel;

public class NovelSearchResponse
{
    [JsonPropertyName("novels")]
    public List<NovelInfo> Novels { get; set; } = new();

    [JsonPropertyName("pages")]
    public int Pages { get; set; }
}
