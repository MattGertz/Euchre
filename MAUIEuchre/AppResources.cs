using System.Resources;

namespace MAUIEuchre
{
    internal static class AppResources
    {
        private static readonly ResourceManager _rm =
            new ResourceManager("MAUIEuchre.Resources.Strings.Resources", typeof(AppResources).Assembly);

        public static ResourceManager ResourceManager => _rm;

        public static string GetString(string name) => _rm.GetString(name) ?? string.Empty;
    }
}
