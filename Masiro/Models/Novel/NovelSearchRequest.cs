namespace Masiro.Models.Novel;

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
