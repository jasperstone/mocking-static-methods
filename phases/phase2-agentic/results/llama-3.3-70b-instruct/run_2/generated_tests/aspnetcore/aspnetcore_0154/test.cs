using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_LogsWarningForDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            var optionsMock = new Mock<WebHostOptions>();
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(new[] { "Assembly1", "Assembly1" });

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder._options = optionsMock.Object;

            // Act
            webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}
