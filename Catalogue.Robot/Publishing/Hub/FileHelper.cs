using System;
using System.IO;

namespace Catalogue.Robot.Publishing.Hub
{
    public class FileHelper : IFileHelper
    {
        public long GetFileSizeInBytes(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public string GetFileExtensionWithoutDot(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);

            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                return fileExtension.ToLower().Replace(".", "");
            }

            return null;
        }

        public string GetBase64String(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);

            return Convert.ToBase64String(bytes);
        }
    }
}
