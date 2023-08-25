using System;

namespace Masiro.reference
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