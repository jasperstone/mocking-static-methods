using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

public class GenerateRazorPageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenFilesAreGenerated()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
        var commandLineArgsMock = new Mock<CommandLineArgs>();
        var razorProjectEngineMock = new Mock<RazorProjectEngine>();

        var generateRazorPage = new GenerateRazorPage
        {
            Logger = loggerMock.Object
        };

        var results = new List<RazorPageGeneratorResult>
        {
            new RazorPageGeneratorResult { FilePath = "path1", GeneratedCode = "code1" },
            new RazorPageGeneratorResult { FilePath = "path2", GeneratedCode = "code2" }
        };

        razorProjectEngineMock.Setup(x => x.Process(It.IsAny<RazorProjectItem>()))
            .Returns(new RazorCodeDocument());

        // Act
        await generateRazorPage.ExecuteAsync(commandLineArgsMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("files successfully generated")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenNoFilesAreFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
        var commandLineArgsMock = new Mock<CommandLineArgs>();
        var razorProjectEngineMock = new Mock<RazorProjectEngine>();

        var generateRazorPage = new GenerateRazorPage
        {
            Logger = loggerMock.Object
        };

        razorProjectEngineMock.Setup(x => x.FileSystem.EnumerateItems(It.IsAny<string>()))
            .Returns(new List<RazorProjectItem>());

        // Act
        await generateRazorPage.ExecuteAsync(commandLineArgsMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("No .cshtml or .razor files were found")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
