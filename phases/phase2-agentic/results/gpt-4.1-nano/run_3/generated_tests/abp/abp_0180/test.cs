using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Moq;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Call_LogInformation_With_Correct_Message()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GenerateRazorPage>>();
            var generateRazorPage = new GenerateRazorPage
            {
                Logger = mockLogger.Object
            };

            // Setup a dummy results list
            var dummyResults = new List<RazorPageGeneratorResult>
            {
                new RazorPageGeneratorResult { FilePath = "path1.cshtml", GeneratedCode = "code1" },
                new RazorPageGeneratorResult { FilePath = "path2.cshtml", GeneratedCode = "code2" }
            };

            // Mock MainCore to return dummy results
            var mockMainCore = new Mock<GenerateRazorPage>();
            mockMainCore.Setup(x => x.MainCore(It.IsAny<RazorProjectEngine>(), It.IsAny<string>()))
                .Returns(dummyResults);

            // Mock CreateProjectEngine to return a dummy engine
            var dummyEngine = new Mock<RazorProjectEngine>();
            mockMainCore.Setup(x => x.CreateProjectEngine(It.IsAny<string>(), null))
                .Returns(dummyEngine.Object);

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(msg => msg.Contains("files successfully generated"))), Times.Once);
        }
    }
}
