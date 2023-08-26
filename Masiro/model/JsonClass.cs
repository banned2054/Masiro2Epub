using System.Net;

namespace Masiro.model
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
        public int     Code { get; set; }
        public string  Msg  { get; set; }
        public string? Url  { get; set; }

        public LoginStatusJson()
        {
            Msg = "";
            Url = default;
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