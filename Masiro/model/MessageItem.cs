namespace Masiro.model
{
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

    internal class FileItem
    {
        public string FileName;
        public string FileTitle;


        public FileItem(string fileName, string fileTitle)
        {
            FileName  = fileName;
            FileTitle = fileTitle;
        }
    }
}