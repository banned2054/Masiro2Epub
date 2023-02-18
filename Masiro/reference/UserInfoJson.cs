using System.Net;

namespace Masiro.reference
{
    internal class UserInfoJson
    {
        public string           UserName { get; set; }
        public string           Password { get; set; }
        public CookieCollection Cookie   { get; set; }
    }
}