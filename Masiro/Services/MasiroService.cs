using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Masiro.Models;
using Masiro.Models.Enums;
using Masiro.Utils;
using RestSharp;

namespace Masiro.Services;

public class MasiroService
{
    private readonly CookieCollection _cookies;
    private const string BaseUrl = "https://masiro.me";

    public MasiroService(CookieCollection cookies)
    {
        _cookies = cookies;
    }

    public async Task<NovelSearchResponse> SearchNovelsAsync(
        int page = 1,
        string? keyword = null,
        string? tags = null,
        string? tagsInverse = null,
        NovelStatus? status = null,
        NovelType? ori = null,
        NovelOrder? order = null,
        string? author = null,
        string? translator = null,
        string? collection = null)
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
        var url = $"/admin/loadMoreNovels?{queryString}";

        var result = await NetUtils.MasiroHtml(_cookies, url);

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
        int page = 1,
        string? keyword = null,
        string? tags = null,
        string? tagsInverse = null,
        NovelStatus? status = null,
        NovelType? ori = null,
        NovelOrder? order = null,
        string? author = null,
        string? translator = null,
        string? collection = null,
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
        var url = $"/admin/loadMoreNovels?{queryString}";

        var result = await NetUtils.MasiroHtmlWithBypass(_cookies, url, ownerWindow);

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

    private static string ConvertOrderToString(NovelOrder order)
    {
        return order switch
        {
            NovelOrder.Hot => "hot",
            NovelOrder.New => "new",
            NovelOrder.ThumbUp => "thumb_up",
            _ => "hot"
        };
    }
}
