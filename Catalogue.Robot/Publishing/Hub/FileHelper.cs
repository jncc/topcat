using System;
using System.IO;
using Sparrow.Json;

namespace Catalogue.Robot.Publishing.Hub
{
    public class FileHelper
    {
        public static long GetFileSizeInBytes(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public static string GetFileExtensionWithoutDot(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);

            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                return fileExtension.ToLower().Replace(".", "");
            }

            return null;
        }

        public static string GetBase64String(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);

            return Convert.ToBase64String(bytes);
        }
    }
}
