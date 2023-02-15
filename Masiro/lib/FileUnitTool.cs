using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Masiro.lib
{
    enum FileType
    {
        Image,
        Html
    }

    internal class FileUnitTool
    {
        public static void MakeDir(string filePath)
        {
            Directory.CreateDirectory(filePath);
        }

        public static bool JudgeFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public static string GetRandomFileName(string filePath, string suffix)
        {
            var newFileName = StringUnitTool.GetRandomString(5) + '.' + suffix;
            var newFilePath = filePath                          + '/' + newFileName;
            while (JudgeFileExist(newFilePath))
            {
                newFileName = StringUnitTool.GetRandomString(5) + '.' + suffix;
                newFilePath = filePath                          + '/' + newFileName;
            }

            return newFileName;
        }

        public static string[] SelectFiles(FileType fileType)
        {
            OpenFileDialog openFileDialog = new();
            switch (fileType)
            {
                case FileType.Image:
                {
                    openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
                    break;
                }
                case FileType.Html:
                {
                    openFileDialog.Filter = "Html files (*.html)|*.html|All files (*.*)|*.*";
                    break;
                }
            }

            openFileDialog.Multiselect = true;
            string[] result = Array.Empty<string>();
            if (openFileDialog.ShowDialog() == true)
            {
                result = openFileDialog.FileNames;
            }

            return result;
        }

        public static string SelectFile()
        {
            OpenFileDialog openFileDialog = new()
                                            {
                                                Filter =
                                                    "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",

                                                Multiselect = false
                                            };
            var result = "";
            if (openFileDialog.ShowDialog() == true)
            {
                result = openFileDialog.FileName;
            }

            return result;
        }

        public static string ReadFile(string filePath)
        {
            string ans = File.ReadAllText(filePath);
            return ans;
        }


        public static async Task DownloadFileAsync(string uri, string filePath)
        {
            using HttpClient   client           = new();
            using var          response         = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            await using var    streamToReadFrom = await response.Content.ReadAsStreamAsync();
            await using Stream streamToWriteTo  = File.Open(filePath, FileMode.Create);
            await streamToReadFrom.CopyToAsync(streamToWriteTo);
        }

        public static void WriteFile(string filePath, string str)
        {
            using StreamWriter writer = new(filePath);
            writer.Write(str);
            writer.Close();
        }

        public static void MakeFile(string filePath)
        {
            var newFile = File.Create(filePath);
            newFile.Close();
        }

        public static FileSystemInfo[] GetFileSystemInfos(string rootPath)
        {
            DirectoryInfo    dir = new(rootPath);
            FileSystemInfo[] ans = dir.GetFileSystemInfos();
            return ans;
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void DeleteDirectory(string path)
        {
            // 如果文件夹存在则进入目录下
            if (Directory.Exists(path))
            {
                // 返回所有文件及目录
                foreach (string p in Directory.GetFileSystemEntries(path))
                {
                    if (File.Exists(p))
                    {
                        // 删除文件
                        File.Delete(p);
                    }
                    else
                    {
                        // 删除子目录
                        DeleteDirectory(p);
                    }
                }

                // 删除当前空目录
                Directory.Delete(path, true);
            }
        }
    }
}