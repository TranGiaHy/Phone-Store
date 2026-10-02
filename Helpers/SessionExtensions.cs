using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PhoneStore.Web.Helpers
{
    public static class SessionExtensions
    {
        // Hàm lưu dữ liệu Giỏ hàng vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm lấy dữ liệu Giỏ hàng từ Session ra
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}