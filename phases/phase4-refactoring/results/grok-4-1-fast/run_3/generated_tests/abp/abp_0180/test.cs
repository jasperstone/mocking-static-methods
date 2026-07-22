using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Log_Information_When_Files_Are_Generated()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("files successfully generated.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var command = new GenerateRazorPageTestImpl(mockLogger.Object);
        command.SetupMainCoreReturnValue(new List<object>
        {
            new { FilePath = "test1", GeneratedCode = "code" },
            new { FilePath = "test2", GeneratedCode = "code" }
        });

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("2 files successfully generated.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class GenerateRazorPageTestImpl : GenerateRazorPage
    {
        private readonly List<object> _mainCoreReturnValue;

        public GenerateRazorPageTestImpl(ILogger<GenerateRazorPage> logger)
        {
            Logger = logger;
            _mainCoreReturnValue = new List<object>();
        }

        public void SetupMainCoreReturnValue(List<object> returnValue)
        {
            _mainCoreReturnValue.Clear();
            _mainCoreReturnValue.AddRange(returnValue);
        }

        private new List<object> MainCore(dynamic projectEngine, string targetProjectDirectory)
        {
            return _mainCoreReturnValue;
        }
    }
}
