using System.Collections.Generic;

namespace Masiro.model
{
    public class ImagePath
    {
        public ImagePath()
        {
            Path = "";
        }

        public string Path { get; set; }
    }

    public class Episode
    {
        public Episode()
        {
            Title  = "";
            SubUrl = "";
        }

        public Episode(Episode oldEpisode)
        {
            Title  = oldEpisode.Title[..];
            SubUrl = oldEpisode.SubUrl[..];
        }

        public Episode(string title, string subUrl)
        {
            Title  = title;
            SubUrl = subUrl;
        }

        public string Title  { get; set; }
        public string SubUrl { get; set; }
    }

    public class Chapter
    {
        public Chapter(string bookTitle)
        {
            BookTitle   = bookTitle;
            EpisodeList = new List<Episode>();
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
}