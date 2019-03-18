using System;
using System.IO;
using Catalogue.Utilities.DriveMapping;

namespace Catalogue.Robot.Publishing.Hub
{
    public class FileHelper : IFileHelper
    {
        public long GetFileSizeInBytes(string filePath)
        {
            var uncPath = JnccDriveMappings.GetUncPath(filePath);
            return new FileInfo(uncPath).Length;
        }

        public string GetFileExtensionWithoutDot(string filePath)
        {
            var uncPath = JnccDriveMappings.GetUncPath(filePath);
            var fileExtension = Path.GetExtension(uncPath);

            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                return fileExtension.ToLower().Replace(".", "");
            }

            return null;
        }

        public string GetBase64String(string filePath)
        {
            var uncPath = JnccDriveMappings.GetUncPath(filePath);
            var bytes = File.ReadAllBytes(uncPath);

            return Convert.ToBase64String(bytes);
        }
    }
}
