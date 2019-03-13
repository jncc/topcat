namespace Catalogue.Robot.Publishing.Hub
{
    public interface IFileHelper
    {
        long GetFileSizeInBytes(string filePath);
        string GetFileExtensionWithoutDot(string filePath);
        string GetBase64String(string filePath);
    }
}
