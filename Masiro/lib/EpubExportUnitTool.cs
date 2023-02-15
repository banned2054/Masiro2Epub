using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Masiro.lib
{
    internal class EpubExportUnitTool
    {
        public static string MakeDir(string rootPath)
        {
            var dirName = StringUnitTool.GetRandomString(5);
            var dirPath = $"{rootPath}/{dirName}";
            while (FileUnitTool.JudgeFileExist(dirPath))
            {
                dirName = StringUnitTool.GetRandomString(5);
                dirPath = $"{rootPath}/{dirName}";
            }

            FileUnitTool.MakeDir(dirPath);
            FileUnitTool.MakeDir($"{dirPath}/META-INF");
            FileUnitTool.MakeDir($"{dirPath}/OEBPS");
            FileUnitTool.MakeDir($"{dirPath}/OEBPS/Images");
            FileUnitTool.MakeDir($"{dirPath}/OEBPS/Styles");
            FileUnitTool.MakeDir($"{dirPath}/OEBPS/Text");

            FileUnitTool.MakeFile($"{dirPath}/OEBPS/Styles/style.css");
            FileUnitTool.WriteFile($"{dirPath}/OEBPS/Styles/style.css", reference.HeadReference.StyleCss);


            FileUnitTool.MakeFile($"{dirPath}/META-INF/container.xml");
            FileUnitTool.WriteFile($"{dirPath}/META-INF/container.xml", reference.HeadReference.ContainerXmlHead);


            FileUnitTool.MakeFile($"{dirPath}/mimetype");
            FileUnitTool.WriteFile($"{dirPath}/mimetype", "application/epub+zip");
            return dirPath;
        }

        public static async Task<string> MakeSection(string rootPath, string htmlPath, int index, int length)
        {
            string originText = FileUnitTool.ReadFile(htmlPath);
            string finalText  = reference.HeadReference.SectionXmlHead;

            string imageDirPath   = $"{rootPath}/OEBPS/Images";
            string sectionDirPath = $"{rootPath}/OEBPS/Text";

            string            title    = StringUnitTool.GetTitle(originText);
            List<MessageItem> bodyList = StringUnitTool.GetBody(originText);

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
                    var newImageName = FileUnitTool.GetRandomFileName(imageDirPath, "jpg");
                    var newImagePath = $"{imageDirPath}/{newImageName}";
                    FileUnitTool.MakeFile(newImagePath);
                    await FileUnitTool.DownloadFileAsync(body.ItemValue, newImagePath);
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
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
            return newFileName;
        }

        public static void MakeImagePage(string rootPath, string oldImagePath, int index, int length)
        {
            var pageDirPath = $"{rootPath}/OEBPS/Text";
            var finalText   = GetImageText(rootPath, oldImagePath, "彩页", "");

            int maxLength   = (int)Math.Log10(length);
            int nowLength   = (int)Math.Log10(index);
            var newFileName = "illustration";
            for (var i = 0; i < maxLength - nowLength; i++)
            {
                newFileName += '0';
            }

            newFileName = newFileName + index + ".xhtml";

            var newFilePath = $"{pageDirPath}/{newFileName}";
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
        }

        public static void MakeCoverPage(string rootPath, string oldImagePath)
        {
            var finalText   = GetImageText(rootPath, oldImagePath, "封面", "cover.jpg");
            var newFilePath = $"{rootPath}/OEBPS/Text/cover.xhtml";
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
        }

        public static void MakeContentsXhtml(string rootPath, List<FileItem> titleList)
        {
            var finalText = reference.HeadReference.SectionXmlHead;
            finalText += "<title>目录</title></head><body><div><h2>目录</h2>";
            foreach (var title in titleList)
            {
                finalText += "<p class=\"noindent\"><span style=\"font-weight: bold;\">"   +
                             $"<a href=\"../Text/{title.FileName}\">{title.FileTitle}</a>" +
                             "<br /></span></p>";
            }

            finalText += "</div></body></html>";
            var newFilePath = $"{rootPath}/OEBPS/Text/contents.xhtml";
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
        }

        public static void MakeToc(string rootPath, List<FileItem> titleList, string bookTitle)
        {
            var pageDirPath = $"{rootPath}/OEBPS/Text";
            var finalText   = reference.HeadReference.TocXmlHead;
            finalText += $"<text>{bookTitle}</text></docTitle><navMap>";
            var fileList = FileUnitTool.GetFileSystemInfos(pageDirPath);
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
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
        }

        public static void MakeContentOpf(string rootPath, string bookTitle)
        {
            var pageDirPath  = $"{rootPath}/OEBPS/Text";
            var imageDirPath = $"{rootPath}/OEBPS/Images";
            var finalText    = reference.HeadReference.ContentXmlHead;
            finalText += $"<dc:title>{bookTitle}</dc:title><meta name=\"Sigil version\" content=\"0.9.9\" />"        +
                         "<meta name=\"cover\" content=\"cover.jpg\" /></metadata><manifest>"                        +
                         "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>"               +
                         "<item id=\"cover.xhtml\" href=\"Text/cover.xhtml\" media-type=\"application/xhtml+xml\"/>" +
                         "";
            var fileList = FileUnitTool.GetFileSystemInfos(pageDirPath).ToList();
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
            fileList  =  FileUnitTool.GetFileSystemInfos(imageDirPath).ToList();
            foreach (var imageFile in fileList)
            {
                finalText += $"<item id=\"{imageFile.Name}\" href=\"Images/{imageFile.Name}\"" +
                             " media-type=\"image/jpeg\"/>";
            }

            finalText += "</manifest><spine toc=\"ncx\"><itemref idref=\"cover.xhtml\"/>";
            fileList  =  FileUnitTool.GetFileSystemInfos(pageDirPath).ToList();
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
            FileUnitTool.MakeFile(newFilePath);
            FileUnitTool.WriteFile(newFilePath, finalText);
        }

        private static string GetImageText(string rootPath, string oldImagePath, string title, string fileName)
        {
            var finalText = reference.HeadReference.SectionXmlHead;

            var imageDirPath = $"{rootPath}/OEBPS/Images";
            var newImageName = fileName;
            if (newImageName == "")
            {
                newImageName = FileUnitTool.GetRandomFileName(imageDirPath, "jpg");
            }

            var newImagePath = $"{imageDirPath}/{newImageName}";
            File.Copy(oldImagePath, newImagePath);

            finalText += $"<title>{title}</title></head><body><div>"                               +
                         "<div class=\"duokan-image-single illus\">"                               +
                         $"<img src=\"../Images/{newImageName}\" alt = \"{newImageName}\"/></div>" +
                         "</div></body></html>";
            return finalText;
        }
    }
}