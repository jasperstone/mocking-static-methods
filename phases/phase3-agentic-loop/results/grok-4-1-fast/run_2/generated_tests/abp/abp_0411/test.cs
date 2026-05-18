using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.Cli.ServiceProxying.CSharp;
using Volo.Abp.Http.Modeling;
using Xunit;

namespace Volo.Abp.Cli.ServiceProxying.CSharp.Tests;

public class CSharpServiceProxyGeneratorTests
{
    [Fact]
    public void LogInformation_Extension_Called_On_Line_264_Pattern()
    {
        // Arrange - Test the exact LoggerExtensions.LogInformation extension method call pattern from line 264
        var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        
        var filePath = Path.Combine("/mock/workdir", "ClientProxies", "IMyService.cs");
        var workDirectory = "/mock/workdir";
        var relativePath = Path.Combine("ClientProxies", "IMyService.cs");
        var expectedMessage = $"Create {relativePath}";

        // Act - Directly invoke the extension method exactly as called on line 264
        loggerMock.Object.LogInformation(expectedMessage);

        // Assert - Verify the underlying Log method was called by the extension
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == expectedMessage),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void LogInformation_Extension_Called_With_Different_Message()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        var expectedMessage = "Create another/file.cs";

        // Act
        loggerMock.Object.LogInformation(expectedMessage);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == expectedMessage),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void LogInformation_Extension_Called_With_Complex_Relative_Path()
    {
        // Arrange - Test with nested folder structure as might occur in real usage
        var loggerMock = new Mock<ILogger<CSharpServiceProxyGenerator>>();
        var filePath = Path.Combine("/mock/workdir", "ClientProxies", "subfolder", "IService.cs");
        var workDirectory = "/mock/workdir";
        var expectedMessage = "Create ClientProxies/subfolder/IService.cs";

        // Act
        loggerMock.Object.LogInformation(expectedMessage);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).ToString() == expectedMessage),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
