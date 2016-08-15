using System.Text;

namespace Utils
{
    public static class ExtensionFunctions
    {
        public static string ConvertToString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
