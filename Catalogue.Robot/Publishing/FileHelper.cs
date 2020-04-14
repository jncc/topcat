using System;
using System.IO;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Utilities.DriveMapping;

namespace Catalogue.Robot.Publishing
{
    public interface IFileHelper
    {
        long GetFileSizeInBytes(string filePath);
        string GetFileExtensionWithoutDot(string filePath);
        string GetBase64String(string filePath);
        bool IsPdfFile(string filePath);
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

        public bool IsPdfFile(string filePath)
        {
            if (!Uri.TryCreate(filePath, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != Uri.UriSchemeFile) return false;

            var extension = GetFileExtensionWithoutDot(filePath);

            return !string.IsNullOrWhiteSpace(extension) && extension.ToLower().Equals("pdf");
        }
    }
}
