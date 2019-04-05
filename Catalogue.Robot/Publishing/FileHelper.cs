using System;
using System.IO;
using Catalogue.Utilities.DriveMapping;

namespace Catalogue.Robot.Publishing.Hub
{
    public interface IFileHelper
    {
        long GetFileSizeInBytes(string filePath);
        string GetFileExtensionWithoutDot(string filePath);
        string GetBase64String(string filePath);
    }

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
