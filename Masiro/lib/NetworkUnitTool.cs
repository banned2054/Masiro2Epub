using BrotliSharpLib;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Masiro.lib
{
    class NetworkUnitTool
    {
        public static async Task<Token> GetToken()
        {
            var client  = new RestClient("https://masiro.me");
            var request = new RestRequest("/admin/auth/login");

            request.AddHeader("Accept",
                              "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("User-Agent",
                              "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new Token($"fail:{response.StatusCode}", new CookieCollection());
            }

            var contentEncoding = response.Headers?.FirstOrDefault(h => h.Name != null &&
                                                                        h.Name.Equals("Content-Encoding",
                                                                            StringComparison.OrdinalIgnoreCase))
                                         ?.Value
                                         ?.ToString();
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

        public static async Task<Token> LoginMasiro(Token nowToken, string userName, string password)
        {
            var client = new RestClient("https://masiro.me");
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

            var loginPostResponse = await client.ExecuteAsync(request);
            if (loginPostResponse.StatusCode != HttpStatusCode.OK)
            {
                if (loginPostResponse.Cookies != null)
                    return new Token($"fail:{loginPostResponse.StatusCode}", loginPostResponse.Cookies);
            }

            return loginPostResponse.Cookies != null
                       ? new Token("success", loginPostResponse.Cookies)
                       : new Token("success", new CookieCollection());
        }

        public static async Task<string> MasiroHtml(CookieCollection cookies, string subUrl)
        {
            var client          = new RestClient("https://masiro.me");
            var cookieHeader    = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
            var getNovelRequest = new RestRequest(subUrl);
            getNovelRequest.AddHeader("Accept",
                                      "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            getNovelRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            getNovelRequest.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            getNovelRequest.AddHeader("User-Agent",
                                      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            getNovelRequest.AddHeader("Cookie", cookieHeader);

            var novelResponse = await client.ExecuteAsync(getNovelRequest);

            if (novelResponse.StatusCode != HttpStatusCode.OK)
            {
                return $"fail:{novelResponse.StatusDescription}";
            }

            return novelResponse.Content ?? "fail:No content";
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