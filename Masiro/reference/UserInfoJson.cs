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
}