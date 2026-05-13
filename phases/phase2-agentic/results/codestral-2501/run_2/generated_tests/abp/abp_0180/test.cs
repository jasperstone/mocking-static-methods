using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenFilesGenerated()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GenerateRazorPage>>();
            var commandLineArgs = new CommandLineArgs("generate-razor-page");
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = loggerMock.Object
            };

            var results = new List<RazorPageGeneratorResult>
            {
                new RazorPageGeneratorResult { FilePath = "path1", GeneratedCode = "code1" },
                new RazorPageGeneratorResult { FilePath = "path2", GeneratedCode = "code2" }
            };

            var mainCoreMock = new Mock<Func<RazorProjectEngine, string, List<RazorPageGeneratorResult>>>();
            mainCoreMock.Setup(m => m(It.IsAny<RazorProjectEngine>(), It.IsAny<string>())).Returns(results);

            var createProjectEngineMock = new Mock<Func<string, RazorProjectEngine>>();
            createProjectEngineMock.Setup(m => m(It.IsAny<string>())).Returns(new RazorProjectEngine());

            var generateRazorPageReflection = typeof(GenerateRazorPage);
            var mainCoreField = generateRazorPageReflection.GetField("MainCore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mainCoreField.SetValue(generateRazorPage, mainCoreMock.Object);

            var createProjectEngineField = generateRazorPageReflection.GetField("CreateProjectEngine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            createProjectEngineField.SetValue(generateRazorPage, createProjectEngineMock.Object);

            // Act
            await generateRazorPage.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("2 files successfully generated.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
