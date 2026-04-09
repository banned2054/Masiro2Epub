using System.Net;

namespace Masiro.Models.User;

public class UserInfo
{
    public UserInfo()
    {
        UserName = string.Empty;
        Password = string.Empty;
        Cookie = new CookieCollection();
    }

    public UserInfo(string name, string password)
    {
        UserName = name;
        Password = password;
        Cookie = new CookieCollection();
    }

    public string UserName { get; set; }
    public string Password { get; set; }
    public CookieCollection Cookie { get; set; }
}
