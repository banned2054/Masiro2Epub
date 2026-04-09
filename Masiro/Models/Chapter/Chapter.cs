using System.Collections.Generic;

namespace Masiro.Models.Chapter;

public class Chapter
{
    public Chapter(string bookTitle)
    {
        BookTitle = bookTitle;
        EpisodeList = [];
    }

    public Chapter(Chapter oldChapter)
    {
        BookTitle = oldChapter.BookTitle[..];
        EpisodeList = new List<Episode>(oldChapter.EpisodeList);
    }

    public void SetEpisodeList(IEnumerable<Episode> episodeList)
    {
        EpisodeList = new List<Episode>(episodeList);
    }

    public string BookTitle { get; set; }
    public List<Episode> EpisodeList { get; set; }
}
