using System;

namespace Masiro.Models;

internal class ArgumentConvey
{
    public class ChapterHandleArgs(Chapter chapter) : EventArgs
    {
        public Chapter NowChapter { get; } = chapter;
    }
}