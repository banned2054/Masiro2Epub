using BrotliSharpLib;
using HandyControl.Controls;
using Masiro.reference;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masiro.lib
{
    class NetworkUnitTool
    {
        public static async Task<bool> IsImageUrl(string url)
        {
            var settingJsonText = FileUnitTool.ReadFile("data/setting.json");
            var settingJson     = JsonUtility.FromJson<SettingJson>(settingJsonText) ?? new SettingJson();

            var httpClientHandler = new HttpClientHandler();

            if (settingJson.UseProxy)
            {
                httpClientHandler = new HttpClientHandler()
                {
                    Proxy    = new WebProxy($"http://{settingJson.ProxyUrl}:{settingJson.ProxyPort}"),
                    UseProxy = true
                };
            }

            if (settingJson.UseUnsaveUrl)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            using var client = new HttpClient(httpClientHandler);

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                return contentType != null && contentType.StartsWith("image/");
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    await using var stream = ex.Response.GetResponseStream();
                    using var       reader = new StreamReader(stream);
                    var             error  = await reader.ReadToEndAsync();
                    MessageBox.Show(error);
                    return false;
                }
            }

            MessageBox.Show("请检查代理是否有问题");
            return false;
        }

        public static async Task<Token> GetToken()
        {
            var settingJsonText = FileUnitTool.ReadFile("data/setting.json");
            var settingJson     = JsonUtility.FromJson<SettingJson>(settingJsonText) ?? new SettingJson();

            var options = new RestClientOptions("https://masiro.me");

            if (settingJson.UseProxy)
            {
                options.Proxy = new WebProxy($"http://{settingJson.ProxyUrl}:{settingJson.ProxyPort}");
            }

            if (settingJson.UseUnsaveUrl)
            {
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            var client = new RestClient(options);


            var request = new RestRequest("/admin/auth/login");

            request.AddHeader("Accept",
                              "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("User-Agent",
                              "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");


            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new Token($"fail:{response.StatusCode}", new CookieCollection());
                }

                string? contentEncoding = null;

                if (response.Headers != null)
                {
                    foreach (var h in response.Headers)
                    {
                        if (h.Name == null || !h.Name.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase))
                            continue;
                        contentEncoding = h.Value?.ToString();
                        break; // Exit the loop once the desired header is found
                    }
                }

                if (contentEncoding != null && contentEncoding.Contains("br"))
                {
                    var compressedBytes = response.RawBytes;
                    if (compressedBytes != null)
                    {
                        var uncompressedBytes  = Brotli.DecompressBuffer(compressedBytes, 0, compressedBytes.Length);
                        var uncompressedString = System.Text.Encoding.UTF8.GetString(uncompressedBytes);
                        response.Content = uncompressedString;
                    }
                }

                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(response.Content);
                var token = htmlDoc.DocumentNode.SelectSingleNode("//input[@class='csrf']/@value")
                                  ?.GetAttributeValue("value", string.Empty);
                if (token == null) return new Token();
                return response.Cookies != null ? new Token(token, response.Cookies) : new Token();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    await using var stream = ex.Response.GetResponseStream();
                    using var       reader = new StreamReader(stream);
                    var             error  = await reader.ReadToEndAsync();
                    return new Token($"fail:{error}", new CookieCollection());
                }
            }

            return new Token($"fail:请检查网址和代理是否有问题", new CookieCollection());
        }

        public static async Task<Token> LoginMasiro(Token nowToken, string userName, string password)
        {
            var settingJsonText = FileUnitTool.ReadFile("data/setting.json");
            var settingJson     = JsonUtility.FromJson<SettingJson>(settingJsonText) ?? new SettingJson();

            var options = new RestClientOptions("https://masiro.me");

            if (settingJson.UseProxy)
            {
                options.Proxy = new WebProxy($"http://{settingJson.ProxyUrl}:{settingJson.ProxyPort}");
            }

            if (settingJson.UseUnsaveUrl)
            {
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            var client = new RestClient(options);
            var loginData = new Dictionary<string, string>()
            {
                { "username", userName },
                { "password", password },
                { "remember", "1" },
                { "_token", nowToken.MyToken }
            };

            var request = new RestRequest("/admin/auth/login", Method.Post);

            var cookies      = nowToken.MyCookie;
            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
            request.AddHeader("Accept",
                              "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("User-Agent",
                              "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            request.AddHeader("x-csrf-token",     nowToken.MyToken);
            request.AddHeader("x-requested-with", "XMLHttpRequest");
            request.AddHeader("Cookie",           cookieHeader);


            foreach (var param in loginData)
            {
                request.AddParameter(param.Key, param.Value);
            }

            try
            {
                var loginPostResponse = await client.ExecuteAsync(request);
                if (loginPostResponse.StatusCode != HttpStatusCode.OK)
                {
                    if (loginPostResponse.Cookies != null)
                        return new Token($"fail:{loginPostResponse.StatusCode}", loginPostResponse.Cookies);
                }

                if (loginPostResponse.Content == null)
                    return loginPostResponse.Cookies != null
                        ? new Token("fail:登陆失败，请尝试重新登录或联系开发者", loginPostResponse.Cookies)
                        : new Token("fail:登陆失败，请尝试重新登录或联系开发者", new CookieCollection());
                var loginStats = JsonUtility.FromJson<LoginStatusJson>(loginPostResponse.Content);
                if (loginStats == null)
                    return loginPostResponse.Cookies != null
                        ? new Token("fail:登陆失败，请尝试重新登录或联系开发者", loginPostResponse.Cookies)
                        : new Token("fail:登陆失败，请尝试重新登录或联系开发者", new CookieCollection());
                if (loginStats.code == 1)
                {
                    return loginPostResponse.Cookies != null
                        ? new Token("success", loginPostResponse.Cookies)
                        : new Token("success", new CookieCollection());
                }

                return new Token($"fail:{loginStats.msg}", new CookieCollection());
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    await using var stream = ex.Response.GetResponseStream();
                    using var       reader = new StreamReader(stream);
                    var             error  = await reader.ReadToEndAsync();
                    return new Token($"fail:{error}", new CookieCollection());
                }
            }

            return new Token($"fail:请检查网址和代理是否有问题", new CookieCollection());
        }

        public static async Task<Token> MasiroHtml(CookieCollection cookies, string subUrl)
        {
            var settingJsonText = FileUnitTool.ReadFile("data/setting.json");
            var settingJson     = JsonUtility.FromJson<SettingJson>(settingJsonText) ?? new SettingJson();

            var options = new RestClientOptions("https://masiro.me");

            if (settingJson.UseProxy)
            {
                options.Proxy = new WebProxy($"http://{settingJson.ProxyUrl}:{settingJson.ProxyPort}");
            }

            if (settingJson.UseUnsaveUrl)
            {
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            var client          = new RestClient(options);
            var cookieHeader    = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
            var getNovelRequest = new RestRequest(subUrl);
            getNovelRequest.AddHeader("Accept",
                                      "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            getNovelRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            getNovelRequest.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            getNovelRequest.AddHeader("User-Agent",
                                      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            getNovelRequest.AddHeader("Cookie", cookieHeader);
            try
            {
                var novelResponse = await client.ExecuteAsync(getNovelRequest);

                if (novelResponse.StatusCode != HttpStatusCode.OK)
                {
                    if (novelResponse.Cookies != null)
                        return new Token($"fail:{novelResponse.StatusCode}", novelResponse.Cookies);
                }

                if (novelResponse.Content != null)
                    return novelResponse.Cookies != null
                        ? new Token(novelResponse.Content, novelResponse.Cookies)
                        : new Token(novelResponse.Content, new CookieCollection());

                return novelResponse.Cookies != null
                    ? new Token("fail:没有文字", novelResponse.Cookies)
                    : new Token("fail:没有文字", new CookieCollection());
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    await using var stream = ex.Response.GetResponseStream();
                    using var       reader = new StreamReader(stream);
                    var             error  = await reader.ReadToEndAsync();
                    return new Token($"fail:{error}", new CookieCollection());
                }
            }

            return new Token($"fail:请检查网址和代理是否有问题", new CookieCollection());
        }

        public class Token
        {
            public Token()
            {
                MyToken  = "";
                MyCookie = new CookieCollection();
            }

            public Token(string token, CookieCollection cookie)
            {
                MyToken  = token;
                MyCookie = cookie;
            }

            public string           MyToken  { get; set; }
            public CookieCollection MyCookie { get; set; }
        }
    }
}