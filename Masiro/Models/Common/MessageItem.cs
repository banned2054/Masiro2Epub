namespace Masiro.Models.Common;

public class MessageItem
{
    public string ItemName = "Text";
    public string ItemValue;

    public MessageItem(string text)
    {
        ItemValue = text;
    }

    public MessageItem(string itemName, string itemValue)
    {
        ItemName = itemName;
        ItemValue = itemValue;
    }
}
