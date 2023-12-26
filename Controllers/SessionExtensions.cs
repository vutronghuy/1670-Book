using System.Text.Json;

namespace _1670_Book.Controllers
{
    public static class SessionExtensions
    {
        public static T Get<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            return data == null ? default(T) : JsonSerializer.Deserialize<T>(data);
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            var data = JsonSerializer.Serialize(value); 
            session.SetString(key, data);
        }
    }
}
