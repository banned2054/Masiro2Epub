using System.Net;

namespace Masiro.Models;

internal class UserInfoJson
{
    public UserInfoJson()
    {
        UserName = string.Empty;
        Password = string.Empty;
        Cookie   = new CookieCollection();
    }

    public UserInfoJson(string name, string password)
    {
        UserName = name;
        Password = password;
        Cookie   = new CookieCollection();
    }

    public string           UserName { get; set; }
    public string           Password { get; set; }
    public CookieCollection Cookie   { get; set; }
}

internal class LoginStatusJson
{
    public int     Code { get; set; }
    public string  Msg  { get; set; } = string.Empty;
    public string? Url  { get; set; }
}

internal class SettingJson
{
    public bool   UseProxy  { get; set; } = false;
    public string ProxyUrl  { get; set; } = string.Empty;
    public int    ProxyPort { get; set; }

    public bool UseUnsaveUrl { get; set; } = false;
}
