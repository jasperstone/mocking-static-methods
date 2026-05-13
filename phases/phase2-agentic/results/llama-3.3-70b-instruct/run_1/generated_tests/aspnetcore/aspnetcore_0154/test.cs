using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_WarnsAboutDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            var optionsMock = new Mock<WebHostOptions>();
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(new[] { "Assembly1", "Assembly1", "Assembly2" });

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, "Development");
            webHostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, "http://localhost:5000");

            // Act
            var host = webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(1));
        }

        [Fact]
        public void Build_DoesNotWarnAboutDuplicateHostingStartupAssemblies_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);
            var optionsMock = new Mock<WebHostOptions>();
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(new[] { "Assembly1", "Assembly1", "Assembly2" });

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, "Development");
            webHostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, "http://localhost:5000");

            // Act
            var host = webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
        }
    }
}
