namespace Masiro.Models;

internal class MessageItem
{
    public string ItemName = "Text";
    public string ItemValue;

    public MessageItem(string text)
    {
        ItemValue = text;
    }

    public MessageItem(string itemName, string itemValue)
    {
        ItemName  = itemName;
        ItemValue = itemValue;
    }
}

internal class FileItem(string fileName, string fileTitle)
{
    public string FileName  = fileName;
    public string FileTitle = fileTitle;
}
