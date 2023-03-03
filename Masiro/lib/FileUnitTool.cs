using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using HandyControl.Controls;
using Masiro.reference;

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
            if (!JudgeFileExist(filePath))
            {
                MakeFile(filePath);
            }

            var ans = File.ReadAllText(filePath);
            return ans;
        }


        public static async Task<string> DownloadFileAsync(string uri, string filePath)
        {
            var settingJsonText = ReadFile("data/setting.json");
            var settingJson     = JsonUtility.FromJson<SettingJson>(settingJsonText) ?? new SettingJson();

            var httpClientHandler = new HttpClientHandler();

            if (settingJson.UseProxy)
            {
                httpClientHandler = new HttpClientHandler()
                                    {
                                        Proxy = new WebProxy($"http://{settingJson.ProxyUrl}:{settingJson.ProxyPort}"),
                                        UseProxy = true
                                    };
            }

            if (settingJson.UseUnsaveUrl)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            using HttpClient     client   = new(httpClientHandler);
            HttpResponseMessage? response = null;

            try
            {
                response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);


                await using var    streamToReadFrom = await response.Content.ReadAsStreamAsync();
                await using Stream streamToWriteTo  = File.Open(filePath, FileMode.Create);

                return await CopyImageToFile(streamToReadFrom, streamToWriteTo, 5);
            }
            catch (HttpRequestException ex) when (ex.InnerException is AuthenticationException)
            {
                MessageBox.Show($"错误：SSL/TLS证书错误，地址：{uri}，此图片将不会下载到epub中");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"错误：{ex.Message}，地址：{uri}，此图片将不会下载到epub中");
            }
            finally
            {
                response?.Dispose();
            }

            return "fail";
        }

        private static async Task<string> CopyImageToFile(Stream streamToReadFrom, Stream streamToWriteTo, int time)
        {
            if (time == 0)
            {
                return "fail";
            }

            var random     = new Random();
            var randomTime = random.Next(0, (int)(2000 * (1.0 - time * 0.2)));
            await Task.Delay(randomTime);
            try
            {
                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                return "success";
            }
            catch
            {
                return await CopyImageToFile(streamToReadFrom, streamToWriteTo, time - 1);
            }
        }

        public static void WriteFile(string filePath, string str)
        {
            if (!JudgeFileExist(filePath))
            {
                MakeFile(filePath);
            }

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