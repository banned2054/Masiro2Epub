namespace Masiro.Models.Settings;

public class AppSettings
{
    public bool UseProxy { get; set; } = false;
    public string ProxyUrl { get; set; } = string.Empty;
    public int ProxyPort { get; set; }

    public bool UseUnsaveUrl { get; set; } = false;
}
