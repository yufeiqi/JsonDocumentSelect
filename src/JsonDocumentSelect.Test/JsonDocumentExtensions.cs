using System.Text.Json;

namespace JsonDocumentSelect.Test
{
    public static class JsonDocumentExtensions
    {
        public static bool DeepEquals(this JsonElement left, JsonElement? right)
        {
            if (right == null)
            {
                return false;
            }
            var jsonString = left.ToString();
            var jsonStringR = right.Value.ToString();
            return jsonString == jsonStringR;
        }

        public static bool DeepEquals(this JsonDocument left, JsonElement? right)
        {
            return DeepEquals(left.RootElement, right);
        }
    }
}
