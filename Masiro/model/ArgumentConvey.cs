using System;

namespace Masiro.model
{
    internal class ArgumentConvey
    {
        public class ChapterHandleArgs : EventArgs
        {
            public Chapter NowChapter { get; }

            public ChapterHandleArgs(Chapter chapter)
            {
                NowChapter = chapter;
            }
        }
    }
}