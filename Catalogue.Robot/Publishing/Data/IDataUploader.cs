
namespace Catalogue.Robot.Publishing.Data
{
    public interface IDataUploader
    {
        void UploadDataFile(string recordId, string filePath);
    }
}
