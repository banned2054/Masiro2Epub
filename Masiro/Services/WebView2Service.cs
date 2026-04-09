using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Masiro.Services;

internal class WebView2Service
{
    private WebView2? _webView;
    private CoreWebView2Environment? _environment;
    private bool _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _environment = await CoreWebView2Environment.CreateAsync(
                null,
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2Data"));

            _webView = new WebView2();
            await _webView.EnsureCoreWebView2Async(_environment);

            _webView.CoreWebView2.Settings.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"初始化 WebView2 失败: {ex.Message}", ex);
        }
    }

    public async Task<string> NavigateAndGetHtmlAsync(string url, int timeoutSeconds = 30)
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null)
        {
            throw new InvalidOperationException("WebView2 尚未初始化");
        }

        var tcs = new TaskCompletionSource<string>();
        var timeout = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

        void NavigationCompletedHandler(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    var html = await _webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                    tcs.TrySetResult(UnescapeJsonString(html));
                });
            }
            else
            {
                tcs.TrySetException(new InvalidOperationException($"导航失败: {e.WebErrorStatus}"));
            }
        }

        _webView.CoreWebView2.NavigationCompleted += NavigationCompletedHandler;

        try
        {
            _webView.CoreWebView2.Navigate(url);

            var completedTask = await Task.WhenAny(tcs.Task, timeout);
            if (completedTask == timeout)
            {
                throw new TimeoutException($"导航超时: {url}");
            }

            return await tcs.Task;
        }
        finally
        {
            _webView.CoreWebView2.NavigationCompleted -= NavigationCompletedHandler;
        }
    }

    public async Task<CookieCollection> GetCookiesAsync(string domain = "https://masiro.me")
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null)
        {
            throw new InvalidOperationException("WebView2 尚未初始化");
        }

        var cookieCollection = new CookieCollection();
        var cookieList = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(domain);

        foreach (var netCookie in cookieList
                    .Select(cookie =>
                                new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain)
                                {
                                    HttpOnly = cookie.IsHttpOnly,
                                    Secure = cookie.IsSecure
                                }))
        {
            cookieCollection.Add(netCookie);
        }

        return cookieCollection;
    }

    public void SetCookies(CookieCollection cookies, string domain = "https://masiro.me")
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null)
        {
            throw new InvalidOperationException("WebView2 尚未初始化");
        }

        foreach (Cookie cookie in cookies)
        {
            var webViewCookie = _webView.CoreWebView2.CookieManager.CreateCookie(
                cookie.Name,
                cookie.Value,
                cookie.Domain,
                cookie.Path);
            webViewCookie.IsHttpOnly = cookie.HttpOnly;
            webViewCookie.IsSecure = cookie.Secure;
            _webView.CoreWebView2.CookieManager.AddOrUpdateCookie(webViewCookie);
        }
    }

    public async Task<bool> IsCloudflareChallengeAsync(string url)
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null)
        {
            return false;
        }

        _webView.CoreWebView2.Navigate(url);

        await Task.Delay(3000);

        var html = await _webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
        html = UnescapeJsonString(html);

        return IsCloudflareChallengePage(html);
    }

    public static bool IsCloudflareChallengePage(string html)
    {
        if (string.IsNullOrEmpty(html)) return false;

        var cloudflareIndicators = new[]
        {
            "cf-browser-verification",
            "cf-challenge-running",
            "Just a moment",
            "Checking your browser",
            "cf-im-under-attack",
            "__cf_bm",
            "cf_clearance",
            "challenge-platform",
            "Turnstile"
        };

        return cloudflareIndicators.Any(indicator =>
            html.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<CloudflareBypassResult> BypassCloudflareAsync(string url, Window? ownerWindow = null)
    {
        if (!_isInitialized || _webView?.CoreWebView2 == null)
        {
            await InitializeAsync();
        }

        var bypassWindow = new Window
        {
            Title = "Cloudflare 验证",
            Width = 800,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = ownerWindow
        };

        var webView = new WebView2();
        bypassWindow.Content = webView;

        await webView.EnsureCoreWebView2Async(_environment);

        webView.CoreWebView2.Settings.UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        var tcs = new TaskCompletionSource<CloudflareBypassResult>();

        webView.CoreWebView2.NavigationCompleted += async (sender, e) =>
        {
            if (!e.IsSuccess) return;

            await Task.Delay(2000);

            var html = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            html = UnescapeJsonString(html);

            if (!IsCloudflareChallengePage(html))
            {
                var cookies = new CookieCollection();
                var cookieList = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://masiro.me");

                foreach (var netCookie in cookieList
                            .Select(cookie =>
                                        new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain)
                                        {
                                            HttpOnly = cookie.IsHttpOnly,
                                            Secure = cookie.IsSecure
                                        }))
                {
                    cookies.Add(netCookie);
                }

                tcs.TrySetResult(new CloudflareBypassResult
                {
                    Success = true,
                    Html = html,
                    Cookies = cookies
                });

                bypassWindow.Close();
            }
        };

        bypassWindow.Closed += (sender, e) =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetResult(new CloudflareBypassResult { Success = false });
            }
        };

        webView.Source = new Uri(url);
        bypassWindow.ShowDialog();

        return await tcs.Task;
    }

    public CoreWebView2? GetCoreWebView2()
    {
        return _webView?.CoreWebView2;
    }

    private static string UnescapeJsonString(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString)) return string.Empty;

        if (jsonString.StartsWith("\"") && jsonString.EndsWith("\""))
        {
            jsonString = jsonString[1..^1];
        }

        return jsonString
            .Replace("\\\"", "\"")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\\", "\\");
    }
}

public class CloudflareBypassResult
{
    public bool Success { get; set; }
    public string Html { get; set; } = string.Empty;
    public CookieCollection Cookies { get; set; } = new();
}
