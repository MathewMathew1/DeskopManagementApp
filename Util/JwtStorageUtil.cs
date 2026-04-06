using System;
using System.IO;

namespace ModernWpfApp.Utils
{
    public static class JwtStorage
    {
        private static readonly string FilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModernWpfApp", "jwt.txt");

        public static void SaveToken(string token)
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(FilePath, token);
        }

        public static string? LoadToken()
        {
            return File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
        }

        public static void ClearToken()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}