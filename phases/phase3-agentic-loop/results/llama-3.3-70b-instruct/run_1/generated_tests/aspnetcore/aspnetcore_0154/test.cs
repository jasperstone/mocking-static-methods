using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var options = new WebHostOptions();
            options.HostingStartupAssemblies.Add("Assembly1");
            options.HostingStartupAssemblies.Add("Assembly1");
            options.HostingStartupAssemblies.Add("Assembly2");

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, "Development");
            webHostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, "http://localhost:5000");

            // Act
            var host = webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(1));
        }

        [Fact]
        public void Build_DoesNotLogWarningForUniqueHostingStartupAssemblies()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            var options = new WebHostOptions();
            options.HostingStartupAssemblies.Add("Assembly1");
            options.HostingStartupAssemblies.Add("Assembly2");

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
