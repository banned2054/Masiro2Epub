using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Masiro.Models;
using Masiro.Reference;

namespace Masiro.Utils;

internal class ExportUtils
{
    public static string MakeDir(string rootPath)
    {
        var dirName = StringUtils.GetRandomString(5);
        var dirPath = $"{rootPath}/{dirName}";
        while (FileUtils.JudgeFileExist(dirPath))
        {
            dirName = StringUtils.GetRandomString(5);
            dirPath = $"{rootPath}/{dirName}";
        }

        FileUtils.MakeDir(dirPath);
        FileUtils.MakeDir($"{dirPath}/META-INF");
        FileUtils.MakeDir($"{dirPath}/OEBPS");
        FileUtils.MakeDir($"{dirPath}/OEBPS/Images");
        FileUtils.MakeDir($"{dirPath}/OEBPS/Styles");
        FileUtils.MakeDir($"{dirPath}/OEBPS/Text");

        FileUtils.MakeFile($"{dirPath}/OEBPS/Styles/style.css");
        FileUtils.WriteFile($"{dirPath}/OEBPS/Styles/style.css", HeadReference.StyleCss);


        FileUtils.MakeFile($"{dirPath}/META-INF/container.xml");
        FileUtils.WriteFile($"{dirPath}/META-INF/container.xml", HeadReference.ContainerXmlHead);


        FileUtils.MakeFile($"{dirPath}/mimetype");
        FileUtils.WriteFile($"{dirPath}/mimetype", "application/epub+zip");
        return dirPath;
    }

    public static async Task<string> MakeSection(string rootPath, string originText, int index, int length)
    {
        var finalText = HeadReference.SectionXmlHead;

        var imageDirPath   = $"{rootPath}/OEBPS/Images";
        var sectionDirPath = $"{rootPath}/OEBPS/Text";

        var title    = StringUtils.GetTitle(originText);
        var bodyList = StringUtils.GetBody(originText);

        finalText += $"<title>{title}</title></head><body><div><h2>{title}</h2>";
        //解析body
        foreach (var body in bodyList)
        {
            if (body.ItemName == "Text")
            {
                finalText += $"<p>{body.ItemValue}</p>";
            }
            else
            {
                var newImageName = FileUtils.GetRandomFileName(imageDirPath, "jpg");
                var newImagePath = $"{imageDirPath}/{newImageName}";
                FileUtils.MakeFile(newImagePath);
                var result = await FileUtils.DownloadFileAsync(body.ItemValue, newImagePath);
                if (result == "fail")
                {
                    continue;
                }

                finalText +=
                    $"<div class=\"duokan-image-single illus\"><img src=\"../Images/{newImageName}\" alt = \"{newImageName}\"/></div>";
            }
        }

        finalText += "</div></body></html>";


        int maxLength = (int)Math.Log10(length);
        int nowLength = (int)Math.Log10(index);


        var newFileName = "Section";
        for (var i = 0; i < maxLength - nowLength; i++)
        {
            newFileName += '0';
        }

        newFileName = newFileName + index + ".xhtml";

        var newFilePath = $"{sectionDirPath}/{newFileName}";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
        return newFileName;
    }

    public static async void MakeImagePage(string rootPath, string oldImagePath, int index, int length)
    {
        var pageDirPath = $"{rootPath}/OEBPS/Text";
        var finalText   = await GetImageText(rootPath, oldImagePath, "彩页", "");

        int maxLength   = (int)Math.Log10(length);
        int nowLength   = (int)Math.Log10(index);
        var newFileName = "illustration";
        for (var i = 0; i < maxLength - nowLength; i++)
        {
            newFileName += '0';
        }

        newFileName = newFileName + index + ".xhtml";

        var newFilePath = $"{pageDirPath}/{newFileName}";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
    }

    public static async void MakeCoverPage(string rootPath, string oldImagePath)
    {
        var finalText   = await GetImageText(rootPath, oldImagePath, "封面", "cover.jpg");
        var newFilePath = $"{rootPath}/OEBPS/Text/cover.xhtml";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
    }

    public static void MakeContentsXhtml(string rootPath, List<FileItem> titleList)
    {
        var finalText = HeadReference.SectionXmlHead;
        finalText += "<title>目录</title></head><body><div><h2>目录</h2>";
        foreach (var title in titleList)
        {
            finalText += "<p class=\"noindent\"><span style=\"font-weight: bold;\">"   +
                         $"<a href=\"../Text/{title.FileName}\">{title.FileTitle}</a>" +
                         "<br /></span></p>";
        }

        finalText += "</div></body></html>";
        var newFilePath = $"{rootPath}/OEBPS/Text/contents.xhtml";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
    }

    public static void MakeToc(string rootPath, List<FileItem> titleList, string bookTitle)
    {
        var pageDirPath = $"{rootPath}/OEBPS/Text";
        var finalText   = HeadReference.TocXmlHead;
        finalText += $"<text>{bookTitle}</text></docTitle><navMap>";
        var fileList = FileUtils.GetFileSystemInfos(pageDirPath);
        int index    = 1;
        foreach (var fileName in fileList)
        {
            if (fileName.Name.Contains("illustration"))
            {
                finalText += "<navPoint id=\"navPoint-1\" playOrder=\"1\">" +
                             "<navLabel>"                                   +
                             "<text>彩页</text>"                              +
                             "</navLabel>"                                  +
                             $"<content src=\"Text/{fileName.Name}\"/>"     +
                             "</navPoint>";
                index++;
                break;
            }
        }

        finalText += $"<navPoint id=\"navPoint-{index}\" playOrder=\"{index++}\">" + "<navLabel>" +
                     "<text>目录</text>" + "</navLabel>" + "<content src=\"Text/contents.xhtml\"/>" + "</navPoint>" +
                     "";
        foreach (var title in titleList)
        {
            finalText += $"<navPoint id=\"navPoint-{index}\" playOrder=\"{index++}\">" +
                         "<navLabel>"                                                  +
                         $"<text>{title.FileTitle}</text>"                             +
                         "</navLabel>"                                                 +
                         $"<content src=\"Text/{title.FileName}\"/>"                   +
                         "</navPoint>";
        }

        finalText += "</navMap></ncx>";
        var newFilePath = $"{rootPath}/OEBPS/toc.ncx";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
    }

    public static void MakeContentOpf(string rootPath, string bookTitle)
    {
        var pageDirPath  = $"{rootPath}/OEBPS/Text";
        var imageDirPath = $"{rootPath}/OEBPS/Images";
        var finalText    = HeadReference.ContentXmlHead;
        finalText += $"<dc:title>{bookTitle}</dc:title><meta name=\"Sigil version\" content=\"0.9.9\" />"        +
                     "<meta name=\"cover\" content=\"cover.jpg\" /></metadata><manifest>"                        +
                     "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>"               +
                     "<item id=\"cover.xhtml\" href=\"Text/cover.xhtml\" media-type=\"application/xhtml+xml\"/>" +
                     "";
        var fileList = FileUtils.GetFileSystemInfos(pageDirPath).ToList();
        int index    = 0;
        while (index < fileList.Count)
        {
            var nowName = fileList[index].Name;
            if (nowName.Contains("cover") || nowName.Contains("contents"))
            {
                fileList.RemoveAt(index);
                continue;
            }

            if (nowName.Contains("illustration"))
            {
                finalText += $"<item id=\"{nowName}\" href=\"Text/{nowName}\"" +
                             " media-type=\"application/xhtml+xml\"/>";
                fileList.RemoveAt(index);
                continue;
            }

            index++;
        }

        finalText += "<item id=\"contents.xhtml\" href=\"Text/contents.xhtml\"" +
                     " media-type=\"application/xhtml+xml\"/>";
        foreach (var nowFile in fileList)
        {
            finalText += $"<item id=\"{nowFile.Name}\" href=\"Text/{nowFile.Name}\"" +
                         " media-type=\"application/xhtml+xml\"/>";
        }

        finalText += "<item id=\"style.css\" href=\"Styles/style.css\" media-type=\"text/css\"/>";
        fileList  =  FileUtils.GetFileSystemInfos(imageDirPath).ToList();
        foreach (var imageFile in fileList)
        {
            finalText += $"<item id=\"{imageFile.Name}\" href=\"Images/{imageFile.Name}\"" +
                         " media-type=\"image/jpeg\"/>";
        }

        finalText += "</manifest><spine toc=\"ncx\"><itemref idref=\"cover.xhtml\"/>";
        fileList  =  FileUtils.GetFileSystemInfos(pageDirPath).ToList();
        index     =  0;
        while (index < fileList.Count)
        {
            var nowName = fileList[index].Name;
            if (nowName.Contains("cover") || nowName.Contains("contents"))
            {
                fileList.RemoveAt(index);
                continue;
            }

            if (nowName.Contains("illustration"))
            {
                finalText += $"<itemref idref=\"{nowName}\"/>";
                fileList.RemoveAt(index);
                continue;
            }

            index++;
        }

        finalText += "<itemref idref=\"contents.xhtml\"/>";
        foreach (var nowFile in fileList)
        {
            finalText += $"<itemref idref=\"{nowFile.Name}\"/>";
        }

        finalText += "</spine><guide>" +
                     "<reference type=\"cover\" title=\"封面\" href=\"Text/cover.xhtml\"/></guide></package>";

        var newFilePath = $"{rootPath}/OEBPS/content.opf";
        FileUtils.MakeFile(newFilePath);
        FileUtils.WriteFile(newFilePath, finalText);
    }

    private static async Task<string> GetImageText(string rootPath, string oldImagePath, string title,
                                                   string fileName)
    {
        var finalText = HeadReference.SectionXmlHead;

        var imageDirPath = $"{rootPath}/OEBPS/Images";
        var newImageName = fileName;
        if (newImageName == "")
        {
            newImageName = FileUtils.GetRandomFileName(imageDirPath, "jpg");
        }

        var newImagePath = $"{imageDirPath}/{newImageName}";
        if (oldImagePath.StartsWith("http"))
        {
            await FileUtils.DownloadFileAsync(oldImagePath, newImagePath);
        }
        else
        {
            File.Copy(oldImagePath, newImagePath);
        }

        finalText += $"<title>{title}</title></head><body><div>"                               +
                     "<div class=\"duokan-image-single illus\">"                               +
                     $"<img src=\"../Images/{newImageName}\" alt = \"{newImageName}\"/></div>" +
                     "</div></body></html>";
        return finalText;
    }
}
