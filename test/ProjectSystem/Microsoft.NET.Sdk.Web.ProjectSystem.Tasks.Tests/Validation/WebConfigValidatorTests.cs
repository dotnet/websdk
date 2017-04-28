using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using Xunit;

namespace Microsoft.NET.Sdk.Web.ProjectSystem.Tasks.Tests.Tasks
{
    public class WebConfigValidatorTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(";")]
        public void WebConfigValidator_NoObsoleteElementsToSearchFor(string elementsToSearchFor)
        {
            // Arrange
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);
            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);

            // Act
            var validator = new WebConfigValidator(
                "",
                elementsToSearchFor,
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }

        [Fact]
        public void WebConfigValidator_NoWebConfig()
        {
            // Arrange
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);
            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);
            mockFileSystem.Setup(x => x.FileExists(@"c:\myproject\SomeFolder\web.config")).Returns(false);

            // Act
            var validator = new WebConfigValidator(
                @"c:\myproject\SomeFolder\project.csproj",
                "appSettings;connectionStrings",
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }

        [Fact]
        public void WebConfigValidator_WebConfigEmpty()
        {
            // Arrange
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);
            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);
            mockFileSystem.Setup(x => x.FileExists(@"c:\myproject\SomeFolder\web.config")).Returns(true);
            mockFileSystem.Setup(x => x.ReadAllText(@"c:\myproject\SomeFolder\web.config")).Returns("");

            // Act
            var validator = new WebConfigValidator(
                @"c:\myproject\SomeFolder\project.csproj",
                "appSettings;connectionStrings",
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }

        [Fact]
        public void WebConfigValidator_WebConfigHasBadFormat()
        {
            // Arrange
            const string webConfigContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <
  <connectionStrings>    
  </connectionStrings>
  <appSettings>    
  </appSettings>
</configuration>";
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);
            mockBuildEngine.Setup(x => x.ProjectFileOfTaskNode).Returns("");
            mockBuildEngine.Setup(x => x.LineNumberOfTaskNode).Returns(0);
            mockBuildEngine.Setup(x => x.ColumnNumberOfTaskNode).Returns(0);
            mockBuildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()));

            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);
            mockFileSystem.Setup(x => x.FileExists(@"c:\myproject\SomeFolder\web.config")).Returns(true);
            mockFileSystem.Setup(x => x.ReadAllText(@"c:\myproject\SomeFolder\web.config")).Returns(webConfigContent);

            // Act
            var validator = new WebConfigValidator(
                @"c:\myproject\SomeFolder\project.csproj",
                "appSettings;connectionStrings",
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }

        [Fact]
        public void WebConfigValidator_WebConfigHasNoObsoleteElements()
        {
            // Arrange
            const string webConfigContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>";
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);

            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);
            mockFileSystem.Setup(x => x.FileExists(@"c:\myproject\SomeFolder\web.config")).Returns(true);
            mockFileSystem.Setup(x => x.ReadAllText(@"c:\myproject\SomeFolder\web.config")).Returns(webConfigContent);

            // Act
            var validator = new WebConfigValidator(
                @"c:\myproject\SomeFolder\project.csproj",
                "appSettings;connectionStrings",
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }

        [Fact]
        public void WebConfigValidator_WebConfigHasObsoleteElements()
        {
            // Arrange
            const string webConfigContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <connectionStrings>    
  </connectionStrings>
  <appSettings1>    
  </appSettings1>
</configuration>";
            var mockBuildEngine = new Mock<IBuildEngine>(MockBehavior.Strict);
            mockBuildEngine.Setup(x => x.LogWarningEvent(It.Is<BuildWarningEventArgs>(y =>
                y.File.Equals(@"c:\myproject\SomeFolder\web.config")
                    && (y.Message.Contains("connectionStrings") && y.LineNumber.Equals(3) && y.ColumnNumber.Equals(4))
                        || (y.Message.Contains("appSettings") && y.LineNumber.Equals(5) && y.ColumnNumber.Equals(4)))));

            var mockFileSystem = new Mock<IFileSystemHelper>(MockBehavior.Strict);
            mockFileSystem.Setup(x => x.FileExists(@"c:\myproject\SomeFolder\web.config")).Returns(true);
            mockFileSystem.Setup(x => x.ReadAllText(@"c:\myproject\SomeFolder\web.config")).Returns(webConfigContent);

            // Act
            var validator = new WebConfigValidator(
                @"c:\myproject\SomeFolder\project.csproj",
                "appSettings;connectionStrings",
                new TaskLoggingHelper(mockBuildEngine.Object, "myTask"),
                mockFileSystem.Object);

            var result = validator.Validate();

            // Assert
            Assert.True(result);
            mockBuildEngine.VerifyAll();
            mockFileSystem.VerifyAll();
        }
    }
}
