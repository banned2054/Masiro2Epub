using System.Text.Json;

namespace Masiro.lib
{
    internal class JsonUtility
    {
        // 反序列化 JSON 格式字符串为对象
        public static T? FromJson<T>(string json)
        {
            if (json == "") return default;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(json, options);
        }

        // 序列化对象为 JSON 格式字符串
        public static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}