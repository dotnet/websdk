namespace Microsoft.NET.Sdk.Web.ProjectSystem.Tasks
{
    public interface IFileSystemHelper
    {
        bool FileExists(string filePath);
        string ReadAllText(string filePath);
    }
}
