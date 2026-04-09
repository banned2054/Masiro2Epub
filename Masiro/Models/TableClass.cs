using System.Collections.Generic;

namespace Masiro.Models;

public class ImagePath
{
    public string Path { get; set; } = string.Empty;
}

public class Episode(string title, string subUrl)
{
    public Episode() : this("", "")
    {
    }

    public Episode(Episode oldEpisode) : this(oldEpisode.Title[..], oldEpisode.SubUrl[..])
    {
    }

    public string Title  { get; set; } = title;
    public string SubUrl { get; set; } = subUrl;
}

public class Chapter
{
    public Chapter(string bookTitle)
    {
        BookTitle   = bookTitle;
        EpisodeList = [];
    }

    public Chapter(Chapter oldChapter)
    {
        BookTitle   = oldChapter.BookTitle[..];
        EpisodeList = new List<Episode>(oldChapter.EpisodeList);
    }

    public void SetEpisodeList(IEnumerable<Episode> episodeList)
    {
        EpisodeList = new List<Episode>(episodeList);
    }

    public string        BookTitle   { get; set; }
    public List<Episode> EpisodeList { get; set; }
}
