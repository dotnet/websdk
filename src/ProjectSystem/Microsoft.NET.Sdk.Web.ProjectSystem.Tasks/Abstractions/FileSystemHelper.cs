using System.IO;

namespace Microsoft.NET.Sdk.Web.ProjectSystem.Tasks
{
    internal class FileSystemHelper : IFileSystemHelper
    {
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
