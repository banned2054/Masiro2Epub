using HtmlAgilityPack;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Masiro.lib
{
    internal class StringUnitTool
    {
        private static readonly Regex TitleHeadRegex     = new("<div[\\u4e00-\\u9fa5\\w \\-]* class=\"box-header");
        private static readonly Regex TitleTailRegex     = new("<div[\\u4e00-\\u9fa5\\w \\-]*=\"novel-header");
        private static readonly Regex BodyHeadRegex      = new("<div[\\u4e00-\\u9fa5\\w \\-]* class=\"box-body");
        private static readonly Regex BodyTailRegex      = new("<div[\\u4e00-\\u9fa5\\w \\-]*=\"likeShare");
        private static readonly Regex ParagraphTagsRegex = new("<p[\\u4e00-\\u9fa5\\w \\-]*[^>]*>");
        private static readonly Regex ImageLinkRegex     = new("src ?[=]? ?\"[.\\w /\\-\\n\\u4e00-\\u9fa5]+");
        private static readonly Regex AllTagsRegex       = new("</?[\\u4e00-\\u9fa5\\w \\-]+[^>]*>");


        private const string CharacterList = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GetTitle(string originText)
        {
            string ans                  = "";
            var    splitByTitleHeadList = TitleHeadRegex.Split(originText);
            if (splitByTitleHeadList.Length > 1)
            {
                var removeHeadText       = splitByTitleHeadList[1];
                var splitByTitleTailList = TitleTailRegex.Split(removeHeadText);
                var removeTailText       = splitByTitleTailList[0];
                var unremovedTitleHead   = removeTailText.Split('>')[0];
                var clearHeadAndTailText = removeTailText[(unremovedTitleHead.Length + 1)..];
                ans = AllTagsRegex.Replace(clearHeadAndTailText, "");
            }

            ans = ClearHeadAndTailSpace(ans.Replace("&nbsp;", ""));

            return ans;
        }


        private static string ClearHeadAndTailSpace(string originText)
        {
            var ans = originText.Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "");
            while (ans.Length > 1)
            {
                if (ans[0] == ' ') ans = ans[1..];
                else break;
            }

            var len = ans.Length;
            while (len > 1)
            {
                if (ans[len - 1] == ' ')
                {
                    ans = ans[..(len - 1)];
                    len--;
                }
                else break;
            }

            if (ans == " ") ans = "";

            return ans;
        }

        public static List<MessageItem> GetBody(string originText)
        {
            List<MessageItem> ans                 = new();
            var               splitByBodyHeadList = BodyHeadRegex.Split(originText.Replace("\n", "").Replace("\r", ""));
            if (splitByBodyHeadList.Length > 1)
            {
                var removeHeadText       = splitByBodyHeadList[1];
                var splitByBodyTailList  = BodyTailRegex.Split(removeHeadText);
                var removeTailText       = splitByBodyTailList[0];
                var unremovedBodyHead    = removeTailText.Split('>')[0];
                var clearHeadAndTailText = removeTailText[(unremovedBodyHead.Length + 1)..];

                var paragraphList = ParagraphTagsRegex.Split(clearHeadAndTailText);

                var flag = true;
                foreach (var paragraph in paragraphList)
                {
                    if (paragraph.Contains("<img"))
                    {
                        var nowSrc     = ImageLinkRegex.Match(paragraph).Value;
                        var imgTail    = nowSrc.Split("files")[1];
                        var imgTrueUrl = "https://www.masiro.me/images/encode" + imgTail;
                        ans.Add(new MessageItem("Image", imgTrueUrl));
                    }
                    else
                    {
                        var now = ClearHeadAndTailSpace(AllTagsRegex.Replace(paragraph, ""));
                        if (now.Length == 0)
                        {
                            if (flag)
                            {
                                flag = false;
                            }
                            else
                            {
                                ans.Add(new MessageItem(" "));
                            }
                        }
                        else
                        {
                            if (now.Length == 1)
                            {
                                if (now[0] != ' ')
                                {
                                    flag = true;
                                    ans.Add(new MessageItem(now));
                                }
                                else
                                {
                                    if (flag)
                                    {
                                        flag = false;
                                    }
                                    else
                                    {
                                        ans.Add(new MessageItem(" "));
                                    }
                                }
                            }
                            else
                            {
                                flag = true;
                                ans.Add(new MessageItem(now));
                            }
                        }
                    }
                }
            }

            return ans;
        }

        public static string GetRandomString(int length)
        {
            StringBuilder stringBuilder = new();
            Random        random        = new();
            while (stringBuilder.Length < length)
            {
                stringBuilder.Append(CharacterList[random.Next(0, CharacterList.Length)]);
            }

            return stringBuilder.ToString();
        }

        static List<Chapter> GetChapterList(string originText)
        {
            var chapterList = new List<Chapter>();
            var htmlDoc     = new HtmlDocument();
            htmlDoc.LoadHtml(originText);
            var ulNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='chapter-ul']");
            if (ulNode == null) return chapterList;
            foreach (var liNode in ulNode.SelectNodes("li"))
            {
                if (liNode.SelectSingleNode(".//ul[@class='episode-ul']") == null)
                {
                    var bookTitle  = liNode.InnerText.Trim()[1..].Replace("&nbsp;", "");
                    var newChapter = new Chapter(bookTitle);
                    chapterList.Add(newChapter);
                }
                else
                {
                    var text = liNode.InnerHtml;

                    if (!text.Contains("episode-ul")) continue;
                    var nowEpisodeList = GetEpisodeList(text);
                    var chapterLength  = chapterList.Count;
                    if (chapterLength > 0)
                    {
                        chapterList[chapterLength - 1].SetEpisodeList(nowEpisodeList);
                    }
                }
            }

            return chapterList;
        }

        private static IEnumerable<Episode> GetEpisodeList(string originText)
        {
            var episodeList = new List<Episode>();
            var htmlDoc     = new HtmlDocument();
            htmlDoc.LoadHtml(originText);
            var ulNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='episode-ul']");

            if (ulNode == null) return episodeList;
            episodeList = (from aNode in ulNode.SelectNodes("a")
                           let href = aNode.GetAttributeValue("href", "")
                           where href != null
                           let aNodeInnerHtml = aNode.InnerHtml
                           let title = GetEpisodeTitle(aNodeInnerHtml)
                           where title != "" && href != ""
                           select new Episode(title, href)).ToList();
            return episodeList;
        }

        private static string GetEpisodeTitle(string originText)
        {
            ZhConverter.Initialize();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originText);
            var liNode    = htmlDoc.DocumentNode.SelectSingleNode("li");
            var spanNodes = liNode?.SelectNodes("span");
            if (spanNodes is not { Count: > 0 }) return "";
            var title = spanNodes[0].InnerText.Trim().Replace("&nbsp;", "");
            title = title.ToHansFromHant();
            return title;
        }

        class Episode
        {
            public Episode(string title, string subUrl)
            {
                Title  = title;
                SubUrl = subUrl;
            }

            public string Title  { get; set; }
            public string SubUrl { get; set; }
        }

        class Chapter
        {
            public Chapter(string bookTitle)
            {
                BookTitle   = bookTitle;
                EpisodeList = new List<Episode>();
            }

            public void SetEpisodeList(IEnumerable<Episode> episodeList)
            {
                EpisodeList = new List<Episode>(episodeList);
            }

            public string        BookTitle   { get; set; }
            public List<Episode> EpisodeList { get; set; }
        }
    }
}