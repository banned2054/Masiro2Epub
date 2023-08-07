using HtmlAgilityPack;
using Masiro.reference;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            originText = originText.Replace("&nbsp;", "");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originText);

            // 删除所有<span>标签并保留其中的内容
            var spanNodes = htmlDoc.DocumentNode.SelectNodes("//span");
            if (spanNodes != null)
            {
                foreach (var spanNode in spanNodes)
                {
                    var parent = spanNode.ParentNode;

                    // 创建一个新的HtmlDocument对象并加载<span>标签的内容
                    var tempDoc = new HtmlDocument();
                    tempDoc.LoadHtml(spanNode.InnerHtml);

                    // 遍历新文档的所有子节点，并将它们插入到原始文档中的适当位置
                    foreach (var childNode in tempDoc.DocumentNode.ChildNodes)
                    {
                        parent.InsertBefore(childNode.CloneNode(true), spanNode);
                    }

                    parent.RemoveChild(spanNode);
                }
            }

            var bodyDivNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box-body')]");
            if (bodyDivNode == null) return ans;
            var lineList = bodyDivNode.SelectNodes(".//p");
            if (lineList == null)
            {
                lineList = bodyDivNode.SelectNodes(".//div");
            }
            else if (lineList.Count == 1)
            {
                lineList = bodyDivNode.SelectNodes(".//div") ?? bodyDivNode.SelectNodes(".//p");
            }

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
                    var line = lineNode.InnerHtml.Trim();
                    if (line == "") continue;
                    var lines = line.Split("<br>");
                    foreach (var ll in lines)
                    {
                        var allQuestionMarks = true;
                        var li               = ll.Trim();
                        foreach (var c in li)
                        {
                            if (c is '?' or '?')
                                continue;
                            allQuestionMarks = false;
                            break;
                        }

                        if (allQuestionMarks) li = "";
                        ans.Add(new MessageItem(li));
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
                    if (!text.Contains("</a>")) continue;
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
            foreach (var aNode in ulNode.SelectNodes(".//a"))
            {
                var href = aNode.GetAttributeValue("href", "");
                if (string.IsNullOrEmpty(href))
                {
                    continue;
                }

                var aNodeInnerHtml = aNode.InnerHtml;
                var title          = GetEpisodeTitle(aNodeInnerHtml);

                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                var episode = new Episode(title, href);
                episodeList.Add(episode);
            }

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