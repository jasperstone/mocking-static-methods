using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
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
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
            var optionsMock = new Mock<IOptions<WebHostOptions>>();
            var hostingStartupAssemblies = new List<string> { "Assembly1", "Assembly1", "Assembly2" };
            optionsMock.Setup(o => o.Value).Returns(new WebHostOptions
            {
                HostingStartupAssemblies = hostingStartupAssemblies
            });

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ILogger<WebHost>>(loggerMock.Object);
                services.AddSingleton<IOptions<WebHostOptions>>(optionsMock.Object);
            });

            // Act
            var host = webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(1));
        }
    }
}
