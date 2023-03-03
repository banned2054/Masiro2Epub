using HtmlAgilityPack;
using Masiro.reference;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Masiro.lib
{
    internal class StringUnitTool
    {
        private const string CharacterList = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GetTitle(string originText)
        {
            var ans = "";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originText);
            var titleDivNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box-header')]");
            if (titleDivNode == null) return ans;
            ans = titleDivNode.InnerText.Trim().Replace("&nbsp;", "");
            return ans;
        }

        public static List<MessageItem> GetBody(string originText)
        {
            List<MessageItem> ans = new();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originText);
            var bodyDivNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box-body')]");
            if (bodyDivNode == null) return ans;
            var lineList = bodyDivNode.SelectNodes(".//span[@style]");
            if (lineList == null)
            {
                lineList = bodyDivNode.SelectNodes(".//p");
                if (lineList == null)
                {
                    lineList = bodyDivNode.SelectNodes(".//div");
                }
                else if (lineList.Count == 1)
                {
                    lineList = bodyDivNode.SelectNodes(".//div");
                }
            }
            else if (lineList.Count == 1)
            {
                lineList = bodyDivNode.SelectNodes(".//p");
                if (lineList == null)
                {
                    lineList = bodyDivNode.SelectNodes(".//div");
                }
                else if (lineList.Count == 1)
                {
                    lineList = bodyDivNode.SelectNodes(".//div");
                }
            }

            int a = 1;

            foreach (var lineNode in lineList)
            {
                var imageNode = lineNode.SelectSingleNode(".//img");
                if (imageNode != null)
                {
                    var src = imageNode.GetAttributeValue("src", "");
                    ans.Add(new MessageItem("Image", src));
                }
                else
                {
                    var line = lineNode.InnerText.Trim();
                    if (line        == "") continue;
                    if (line.Length == 1) continue;
                    if (line.All(c => c == '?')) line = "";
                    ans.Add(new MessageItem(line));
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

        public static ObservableCollection<Chapter> GetChapterList(string originText)
        {
            var chapterList = new ObservableCollection<Chapter>();
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
            episodeList = (from aNode in ulNode.SelectNodes(".//a")
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
            var liNode    = htmlDoc.DocumentNode.SelectSingleNode("//li");
            var spanNodes = liNode?.SelectNodes(".//span");
            if (spanNodes is not { Count: > 0 }) return "";
            var title = spanNodes[0].InnerText.Trim().Replace("&nbsp;", "");
            title = title.ToHansFromHant();
            return title;
        }
    }
}