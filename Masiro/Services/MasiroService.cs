using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Masiro.Models.Enums;
using Masiro.Models.Novel;
using Masiro.Utils;

namespace Masiro.Services;

public class MasiroService(CookieCollection cookies)
{
    private const string BaseUrl = "https://masiro.me";

    #region 搜索功能

    public async Task<NovelSearchResponse> SearchNovelsAsync(
        int          page        = 1,
        string?      keyword     = null,
        string?      tags        = null,
        string?      tagsInverse = null,
        NovelStatus? status      = null,
        NovelType?   ori         = null,
        NovelOrder?  order       = null,
        string?      author      = null,
        string?      translator  = null,
        string?      collection  = null)
    {
        var queryParams = new List<string> { $"page={page}" };

        if (!string.IsNullOrEmpty(keyword))
        {
            queryParams.Add($"keyword={HttpUtility.UrlEncode(keyword)}");
        }

        if (!string.IsNullOrEmpty(tags))
        {
            queryParams.Add($"tags={tags}");
        }

        if (!string.IsNullOrEmpty(tagsInverse))
        {
            queryParams.Add($"tags_inverse={tagsInverse}");
        }

        if (status.HasValue)
        {
            queryParams.Add($"status={(int)status.Value}");
        }

        if (ori.HasValue)
        {
            queryParams.Add($"ori={(int)ori.Value}");
        }

        if (order.HasValue)
        {
            queryParams.Add($"order={ConvertOrderToString(order.Value)}");
        }

        if (!string.IsNullOrEmpty(author))
        {
            queryParams.Add($"author={HttpUtility.UrlEncode(author)}");
        }

        if (!string.IsNullOrEmpty(translator))
        {
            queryParams.Add($"translator={HttpUtility.UrlEncode(translator)}");
        }

        if (!string.IsNullOrEmpty(collection))
        {
            queryParams.Add($"collection={collection}");
        }

        var queryString = string.Join("&", queryParams);
        var url         = $"/admin/loadMoreNovels?{queryString}";

        var result = await NetUtils.MasiroHtml(cookies, url);

        if (result.MyToken.StartsWith("fail"))
        {
            throw new InvalidOperationException($"搜索失败: {result.MyToken}");
        }

        var response = JsonUtils.FromJson<NovelSearchResponse>(result.MyToken);
        if (response == null)
        {
            throw new InvalidOperationException("解析搜索结果失败");
        }

        return response;
    }

    public async Task<NovelSearchResponse> SearchNovelsWithBypassAsync(
        int                    page        = 1,
        string?                keyword     = null,
        string?                tags        = null,
        string?                tagsInverse = null,
        NovelStatus?           status      = null,
        NovelType?             ori         = null,
        NovelOrder?            order       = null,
        string?                author      = null,
        string?                translator  = null,
        string?                collection  = null,
        System.Windows.Window? ownerWindow = null)
    {
        var queryParams = new List<string> { $"page={page}" };

        if (!string.IsNullOrEmpty(keyword))
        {
            queryParams.Add($"keyword={HttpUtility.UrlEncode(keyword)}");
        }

        if (!string.IsNullOrEmpty(tags))
        {
            queryParams.Add($"tags={tags}");
        }

        if (!string.IsNullOrEmpty(tagsInverse))
        {
            queryParams.Add($"tags_inverse={tagsInverse}");
        }

        if (status.HasValue)
        {
            queryParams.Add($"status={(int)status.Value}");
        }

        if (ori.HasValue)
        {
            queryParams.Add($"ori={(int)ori.Value}");
        }

        if (order.HasValue)
        {
            queryParams.Add($"order={ConvertOrderToString(order.Value)}");
        }

        if (!string.IsNullOrEmpty(author))
        {
            queryParams.Add($"author={HttpUtility.UrlEncode(author)}");
        }

        if (!string.IsNullOrEmpty(translator))
        {
            queryParams.Add($"translator={HttpUtility.UrlEncode(translator)}");
        }

        if (!string.IsNullOrEmpty(collection))
        {
            queryParams.Add($"collection={collection}");
        }

        var queryString = string.Join("&", queryParams);
        var url         = $"/admin/loadMoreNovels?{queryString}";

        var result = await NetUtils.MasiroHtmlWithBypass(cookies, url, ownerWindow);

        if (result.MyToken.StartsWith("fail"))
        {
            throw new InvalidOperationException($"搜索失败: {result.MyToken}");
        }

        var response = JsonUtils.FromJson<NovelSearchResponse>(result.MyToken);
        if (response == null)
        {
            throw new InvalidOperationException("解析搜索结果失败");
        }

        return response;
    }

    #endregion

    #region 小说内容获取

    public async Task<string> GetNovelHtmlAsync(string subUrl)
    {
        var result = await NetUtils.MasiroHtml(cookies, subUrl);

        if (result.MyToken.StartsWith("fail"))
        {
            throw new InvalidOperationException($"获取小说内容失败: {result.MyToken}");
        }

        return result.MyToken;
    }

    public async Task<NovelContentResult> GetNovelHtmlWithBypassAsync(string                 subUrl,
                                                                      System.Windows.Window? ownerWindow = null)
    {
        var result = await NetUtils.MasiroHtmlWithBypass(cookies, subUrl, ownerWindow);

        if (result.MyToken.StartsWith("fail"))
        {
            return new NovelContentResult
            {
                Success      = false,
                ErrorMessage = result.MyToken,
                Html         = string.Empty,
                Cookies      = result.MyCookie
            };
        }

        return new NovelContentResult
        {
            Success = true,
            Html    = result.MyToken,
            Cookies = result.MyCookie
        };
    }

    public async Task<NovelContentResult> GetNovelHtmlWithAutoLoginAsync(
        string                 subUrl,
        string                 userName,
        string                 password,
        System.Windows.Window? ownerWindow = null)
    {
        var result = await NetUtils.MasiroHtmlWithBypass(cookies, subUrl, ownerWindow);

        if (result.MyToken.StartsWith("fail"))
        {
            var token       = await NetUtils.GetTokenWithBypass(ownerWindow);
            var loginResult = await NetUtils.LoginMasiroWithBypass(token, userName, password, ownerWindow);

            if (loginResult.MyToken != "success")
            {
                return new NovelContentResult
                {
                    Success      = false,
                    ErrorMessage = loginResult.MyToken.StartsWith("fail:") ? loginResult.MyToken[5..] : "登录失败",
                    Html         = string.Empty,
                    Cookies      = loginResult.MyCookie
                };
            }

            var finalResult = await NetUtils.MasiroHtmlWithBypass(loginResult.MyCookie, subUrl, ownerWindow);
            if (finalResult.MyToken.StartsWith("fail"))
            {
                return new NovelContentResult
                {
                    Success      = false,
                    ErrorMessage = finalResult.MyToken,
                    Html         = string.Empty,
                    Cookies      = finalResult.MyCookie
                };
            }

            return new NovelContentResult
            {
                Success = true,
                Html    = finalResult.MyToken,
                Cookies = finalResult.MyCookie
            };
        }

        return new NovelContentResult
        {
            Success = true,
            Html    = result.MyToken,
            Cookies = result.MyCookie
        };
    }

    #endregion

    #region 登录功能

    public static async Task<LoginTokenResult> GetTokenAsync()
    {
        var result = await NetUtils.GetToken();

        if (result.MyToken.StartsWith("fail"))
        {
            return new LoginTokenResult
            {
                Success      = false,
                ErrorMessage = result.MyToken.StartsWith("fail:") ? result.MyToken[5..] : result.MyToken,
                Token        = string.Empty,
                Cookies      = result.MyCookie
            };
        }

        return new LoginTokenResult
        {
            Success = true,
            Token   = result.MyToken,
            Cookies = result.MyCookie
        };
    }

    public static async Task<LoginTokenResult> GetTokenWithBypassAsync(System.Windows.Window? ownerWindow = null)
    {
        var result = await NetUtils.GetTokenWithBypass(ownerWindow);

        if (result.MyToken.StartsWith("fail"))
        {
            return new LoginTokenResult
            {
                Success      = false,
                ErrorMessage = result.MyToken.StartsWith("fail:") ? result.MyToken[5..] : result.MyToken,
                Token        = string.Empty,
                Cookies      = result.MyCookie
            };
        }

        return new LoginTokenResult
        {
            Success = true,
            Token   = result.MyToken,
            Cookies = result.MyCookie
        };
    }

    public static async Task<LoginResult> LoginAsync(string userName, string password)
    {
        var tokenResult = await GetTokenAsync();
        if (!tokenResult.Success)
        {
            return new LoginResult
            {
                Success      = false,
                ErrorMessage = tokenResult.ErrorMessage,
                Cookies      = tokenResult.Cookies
            };
        }

        var token  = new NetUtils.Token(tokenResult.Token, tokenResult.Cookies);
        var result = await NetUtils.LoginMasiro(token, userName, password);

        if (result.MyToken != "success")
        {
            return new LoginResult
            {
                Success      = false,
                ErrorMessage = result.MyToken.StartsWith("fail:") ? result.MyToken[5..] : result.MyToken,
                Cookies      = result.MyCookie
            };
        }

        return new LoginResult
        {
            Success = true,
            Cookies = result.MyCookie
        };
    }

    public static async Task<LoginResult> LoginWithBypassAsync(string                 userName, string password,
                                                               System.Windows.Window? ownerWindow = null)
    {
        var tokenResult = await GetTokenWithBypassAsync(ownerWindow);
        if (!tokenResult.Success)
        {
            return new LoginResult
            {
                Success      = false,
                ErrorMessage = tokenResult.ErrorMessage,
                Cookies      = tokenResult.Cookies
            };
        }

        var token  = new NetUtils.Token(tokenResult.Token, tokenResult.Cookies);
        var result = await NetUtils.LoginMasiroWithBypass(token, userName, password, ownerWindow);

        if (result.MyToken != "success")
        {
            return new LoginResult
            {
                Success      = false,
                ErrorMessage = result.MyToken.StartsWith("fail:") ? result.MyToken[5..] : result.MyToken,
                Cookies      = result.MyCookie
            };
        }

        return new LoginResult
        {
            Success = true,
            Cookies = result.MyCookie
        };
    }

    #endregion

    #region 工具方法

    public static async Task<bool> IsImageUrlAsync(string url)
    {
        return await NetUtils.IsImageUrl(url);
    }

    private static string ConvertOrderToString(NovelOrder order)
    {
        return order switch
        {
            NovelOrder.Hot     => "hot",
            NovelOrder.New     => "new",
            NovelOrder.ThumbUp => "thumb_up",
            _                  => "hot"
        };
    }

    #endregion
}

public class NovelContentResult
{
    public bool             Success      { get; set; }
    public string           Html         { get; set; } = string.Empty;
    public string           ErrorMessage { get; set; } = string.Empty;
    public CookieCollection Cookies      { get; set; } = new();
}

public class LoginTokenResult
{
    public bool             Success      { get; set; }
    public string           Token        { get; set; } = string.Empty;
    public string           ErrorMessage { get; set; } = string.Empty;
    public CookieCollection Cookies      { get; set; } = new();
}

public class LoginResult
{
    public bool             Success      { get; set; }
    public string           ErrorMessage { get; set; } = string.Empty;
    public CookieCollection Cookies      { get; set; } = new();
}
