using Masiro.Models;
using Masiro.Utils;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.Views;

public partial class LoginUserControl : UserControl
{
    public event EventHandler LoginButtonClicked = null!;

    private bool _isInitialized;

    public LoginUserControl()
    {
        _isInitialized = false;
        InitializeComponent();
        Loaded += LoginUserControl_Loaded;
    }

    private async void LoginUserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized) return;

        try
        {
            var env =
                await CoreWebView2Environment.CreateAsync(null,
                                                          System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                                   "WebView2Data"));
            await WebView.EnsureCoreWebView2Async(env);

            WebView.CoreWebView2.Settings.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            WebView.CoreWebView2.Settings.IsStatusBarEnabled            = false;

            WebView.Source = new Uri("https://masiro.me/admin/auth/login");
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化 WebView2 失败: {ex.Message}\n请确保已安装 WebView2 Runtime", "错误", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }

    private async void CompleteLoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (WebView.CoreWebView2 == null)
            {
                MessageBox.Show("浏览器尚未初始化完成，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cookies = await GetCookiesAsync();

            if (!FileUtils.JudgeFileExist("data")) FileUtils.MakeDir("data");

            var jsonString = FileUtils.ReadFile("data/user.json");
            var user       = JsonUtils.FromJson<UserInfoJson>(jsonString) ?? new UserInfoJson();
            user.Cookie = cookies;
            jsonString  = JsonUtils.ToJson(user);
            FileUtils.WriteFile("data/user.json", jsonString);

            LoginButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"获取 Cookie 失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task<CookieCollection> GetCookiesAsync()
    {
        var cookieCollection = new CookieCollection();

        if (WebView.CoreWebView2 == null) return cookieCollection;

        var cookieList = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://masiro.me");

        foreach (var netCookie in cookieList
                    .Select(cookie =>
                                new Cookie(cookie.Name, cookie.Value, cookie.Path,
                                           cookie.Domain)
                                {
                                    HttpOnly = cookie.IsHttpOnly,
                                    Secure   = cookie.IsSecure
                                }))
        {
            cookieCollection.Add(netCookie);
        }

        return cookieCollection;
    }
}
