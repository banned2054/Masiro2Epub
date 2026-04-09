using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using Masiro.Models;
using OpenCC;

namespace Masiro.Utils;

internal class StringUtils
{
    private const string CharacterList = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // 繁体转简体转换器（延迟初始化）
    private static readonly Lazy<Func<string, string>> HantToHansConverter = new(() => OpenCC.OpenCC.Converter("tw", "cn"));

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
                    var li               = ll.Trim();
                    var allQuestionMarks = li.All(c => c is '?' or '?');

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

        // 尝试从 script 标签中获取 JSON 数据（新结构）
        var fChaptersScriptNode = htmlDoc.DocumentNode.SelectSingleNode("//script[@id='f-chapters-json']");
        var chaptersScriptNode  = htmlDoc.DocumentNode.SelectSingleNode("//script[@id='chapters-json']");

        if (fChaptersScriptNode != null && chaptersScriptNode != null)
        {
            var fChaptersJson = fChaptersScriptNode.InnerText;
            var chaptersJson  = chaptersScriptNode.InnerText;

            var fatherChapters = JsonSerializer.Deserialize<List<FatherChapterJson>>(fChaptersJson);
            var episodes       = JsonSerializer.Deserialize<List<EpisodeJson>>(chaptersJson);

            if (fatherChapters != null && episodes != null)
            {
                var converter = HantToHansConverter.Value;
                foreach (var father in fatherChapters)
                {
                    var bookTitle  = converter(father.title);
                    var newChapter = new Chapter(bookTitle);

                    var relatedEpisodes = episodes.Where(e => e.parent_id == father.id).ToList();
                    var episodeList     = new List<Episode>();

                    foreach (var ep in relatedEpisodes)
                    {
                        var title   = converter(ep.title);
                        var subUrl  = $"/admin/novelReading?cid={ep.id}";
                        var episode = new Episode(title, subUrl);
                        episodeList.Add(episode);
                    }

                    newChapter.SetEpisodeList(episodeList);
                    chapterList.Add(newChapter);
                }
            }

            return chapterList;
        }

        // 回退到旧结构的解析方式
        var ulNode = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='chapter-ul']");
        if (ulNode == null) return chapterList;
        var liNodes = ulNode.SelectNodes("li");
        if (liNodes == null) return chapterList;

        foreach (var liNode in liNodes)
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

    // JSON 数据模型
    private class FatherChapterJson
    {
        public int    id       { get; set; }
        public string title    { get; set; } = "";
        public string describe { get; set; } = "";
        public int    limit_lv { get; set; }
    }

    private class EpisodeJson
    {
        public int    id         { get; set; }
        public int    novel_id   { get; set; }
        public int    parent_id  { get; set; }
        public string title      { get; set; } = "";
        public int    creator    { get; set; }
        public string describe   { get; set; } = "";
        public int    limit_lv   { get; set; }
        public int    cost       { get; set; }
        public int    translator { get; set; }
        public int    porter     { get; set; }
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
        var converter = HantToHansConverter.Value;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(originText);
        var liNode    = htmlDoc.DocumentNode.SelectSingleNode("//li");
        var spanNodes = liNode?.SelectNodes(".//span");
        if (spanNodes is not { Count: > 0 }) return "";
        var title = spanNodes[0].InnerText.Trim().Replace("&nbsp;", "");
        title = converter(title);
        return title;
    }
}
