using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Utilities;

namespace Microsoft.NET.Sdk.Web.ProjectSystem.Tasks
{
    public class WebConfigValidator
    {
        private const string WebConfigFileName = "web.config";

        public WebConfigValidator(
            string projectFile, 
            string obsoleteWebConfigElements, 
            TaskLoggingHelper logger)
            : this(projectFile, obsoleteWebConfigElements, logger, new FileSystemHelper())
        {           
        }

        public WebConfigValidator(
            string projectFile, 
            string obsoleteWebConfigElements, 
            TaskLoggingHelper logger, 
            IFileSystemHelper fileSystem)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ProjectFile = projectFile ?? throw new ArgumentException(nameof(projectFile));
            ObsoleteWebConfigElements = obsoleteWebConfigElements;

            FileSystem = fileSystem;
        }

        private string ProjectFile { get; }
        private string ObsoleteWebConfigElements { get; }
        private TaskLoggingHelper Logger { get; }
        private IFileSystemHelper FileSystem { get; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(ObsoleteWebConfigElements))
            {
                return true;
            }

            var elementsToValidate = ObsoleteWebConfigElements.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (elementsToValidate == null || elementsToValidate.Length == 0)
            {
                return true;
            }

            var projectFolder = Path.GetDirectoryName(ProjectFile);
            var webConfigPath = Path.Combine(projectFolder, WebConfigFileName);
            if (!FileSystem.FileExists(webConfigPath))
            {
                return true;
            }

            var webConfigXml = LoadXml(webConfigPath, out string webConfigContent);
            if (webConfigXml == null)
            {
                // if somthing wrong with the format, it is not our task to report that, it would most likely 
                // have been reported before us during build. So just return true here. 
                return true;
            }

            string[] webConfigContentLines = null;
            foreach (var elementName in elementsToValidate)
            {
                var childElement = webConfigXml.Root.Element(elementName);
                if (childElement != null)
                {
                    IXmlLineInfo lineInfo = childElement;
                    var line = 0;
                    var column = 0;
                    if (lineInfo.HasLineInfo())
                    {
                        line = lineInfo.LineNumber;
                        column = lineInfo.LinePosition;
                    }
                    else
                    {
                        if (webConfigContentLines == null)
                        {
                            webConfigContentLines = webConfigContent.Split('\n');
                        }

                        GetLineAndColumnFromContent(webConfigContentLines, elementName, out line, out column);
                    }

                    Logger.LogWarning(string.Empty /*subcategory*/,
                                      string.Empty /*warningCode*/,
                                      string.Empty /*helpKeyword*/,
                                      webConfigPath,
                                      line,
                                      column,
                                      0 /*endLineNumber*/,
                                      0 /*endColumnNumber*/,
                                      string.Format(Resources.ValidateWebConfig_ObsoleteElement, elementName));
                }
            }

            return true;
        }

        private XDocument LoadXml(string filePath, out string content)
        {
            content = FileSystem.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            using (var reader = new StringReader(content))
            {
                try
                {
                    return XDocument.Load(reader);
                }
                catch(Exception e)
                {
                    Logger.LogErrorFromException(e);
                    return null;
                }
            }
        }

        private void GetLineAndColumnFromContent(string[] contentLines, string elementName, out int line, out int column)
        {
            line = 0;
            column = 0;

            for (var i = 0; i < contentLines.Length; ++i)
            {
                var index = contentLines[i].IndexOf(elementName);
                if (index > 0)
                {
                    line = i + 1;
                    column = index + 1;
                    break;
                }
            }
        }
    }
}
