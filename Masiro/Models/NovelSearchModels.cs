using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Masiro.Models;

public class NovelSearchRequest
{
    public int Page { get; set; } = 1;
    public string? Keyword { get; set; }
    public string? Tags { get; set; }
    public string? TagsInverse { get; set; }
    public int? Status { get; set; }
    public int? Ori { get; set; }
    public string? Order { get; set; }
    public string? Author { get; set; }
    public string? Translator { get; set; }
    public string? Collection { get; set; }
}

public class NovelSearchResponse
{
    [JsonPropertyName("novels")]
    public List<NovelInfo> Novels { get; set; } = new();

    [JsonPropertyName("pages")]
    public int Pages { get; set; }
}

public class NovelInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("cover_img")]
    public string CoverImg { get; set; } = string.Empty;

    [JsonPropertyName("words")]
    public int Words { get; set; }

    [JsonPropertyName("hs")]
    public int Hs { get; set; }

    [JsonPropertyName("comment_nums")]
    public int CommentNums { get; set; }

    [JsonPropertyName("thumb_up")]
    public int ThumbUp { get; set; }

    [JsonPropertyName("collect_nums")]
    public int CollectNums { get; set; }

    [JsonPropertyName("new_up_id")]
    public int NewUpId { get; set; }

    [JsonPropertyName("new_up_content")]
    public string NewUpContent { get; set; } = string.Empty;

    [JsonPropertyName("is_ori")]
    public int IsOri { get; set; }

    [JsonPropertyName("translators")]
    public List<TranslatorInfo> Translators { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<TagInfo> Tags { get; set; } = new();

    [JsonPropertyName("lv_limit")]
    public int LvLimit { get; set; }
}

public class TranslatorInfo
{
    [JsonPropertyName("translator")]
    public int TranslatorId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TagInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
