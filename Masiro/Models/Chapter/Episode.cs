namespace Masiro.Models.Chapter;

public class Episode(string title, string subUrl)
{
    public Episode() : this("", "")
    {
    }

    public Episode(Episode oldEpisode) : this(oldEpisode.Title[..], oldEpisode.SubUrl[..])
    {
    }

    public string Title { get; set; } = title;
    public string SubUrl { get; set; } = subUrl;
}
