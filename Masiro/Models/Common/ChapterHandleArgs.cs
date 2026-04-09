using Masiro.Models.Chapter;

namespace Masiro.Models.Common;

public class ChapterHandleArgs(Chapter.Chapter chapter) : System.EventArgs
{
    public Chapter.Chapter NowChapter { get; } = chapter;
}
