using System.Net;

namespace Masiro.reference
{
    internal class UserInfoJson
    {
        public UserInfoJson()
        {
            UserName = "";
            Password = "";
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
        public int     code { get; set; }
        public string  msg  { get; set; }
        public string? url  { get; set; }

        public LoginStatusJson()
        {
            msg = "";
            url = default;
        }
    }

    internal class SettingJson
    {
        public bool   UseProxy  { get; set; }
        public string ProxyUrl  { get; set; }
        public int    ProxyPort { get; set; }

        public bool UseUnsaveUrl { get; set; }

        public SettingJson()
        {
            UseProxy     = false;
            UseUnsaveUrl = false;

            ProxyUrl = "";
        }
    }
}