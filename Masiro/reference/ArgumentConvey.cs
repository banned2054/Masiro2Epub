using System;

namespace Masiro.reference
{
    class ArgumentConvey
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